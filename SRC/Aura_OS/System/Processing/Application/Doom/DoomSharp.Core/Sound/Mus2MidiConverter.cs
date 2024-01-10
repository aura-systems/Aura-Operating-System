using System;
using System.IO;
using System.Text;

namespace DoomSharp.Core.Sound
{
    public class Mus2MidiConverter
    {
        private const int NumberOfChannels = 16;
        private const int MidiPercussionChannel = 9;
        private const int MusPercussionChannel = 15;

        private static readonly byte[] CtrlTranslate =
        {
            0x00, // program change
            0x20, // bank select
            0x01, // modulation pot
            0x07, // volume
            0x0A, // pan pot
            0x0B, // expression pot
            0x5B, // reverb depth
            0x5D, // chorus depth
            0x40, // sustain pedal
            0x43, // soft pedal
            0x78, // all sounds off
            0x7B, // all notes off
            0x7E, // mono
            0x7F, // poly
            0x79 // reset all controllers
        };

        private enum MusEventType
        {
            NoteOff = 0x00,
            NoteOn = 0x10,
            PitchBend = 0x20,
            SysEvent = 0x30,
            CtrlChange = 0x40,
            ScoreEnd = 0x60
        }

        private enum MidiEventType
        {
            NoteOff = 0x80,
            NoteOn = 0x90,
            AfterTouch = 0xA0,
            CtrlChange = 0xB0,
            ProgramChange = 0xC0,
            AfterTouchChannel = 0xD0,
            PitchBend = 0xE0
        }

        private static readonly byte[] MidiHeader =
        {
            (byte)'M', (byte)'T', (byte)'h', (byte)'d',
            0, 0, 0, 6, // header length
            0, 0, // MIDI type 0
            0, 1, // one track
            0, 70 // 70 ticks per quarter note
        };

        private static readonly byte[] MidiTrackHeader =
        {
            (byte)'M', (byte)'T', (byte)'r', (byte)'k',
            0, 0, 0, 0 // header length placeholder
        };

        private readonly byte[] _channelVelocities =
        {
            127, 127, 127, 127, 127, 127, 127, 127,
            127, 127, 127, 127, 127, 127, 127, 127
        };

        private readonly int[] _channelMap = new int[NumberOfChannels];
        private MusHeader _musHeader;

        private uint _queuedTime = 0;
        private uint _trackSize = 0;
        
        private Mus2MidiConverter() {}

        public static bool TryConvert(byte[] data, out byte[] midiFile)
        {
            var converter = new Mus2MidiConverter();
            midiFile = Array.Empty<byte>();

            converter.Initialize();
            using var readStream = new MemoryStream(data, false);
            using var reader = new BinaryReader(readStream, Encoding.ASCII, false);
            
            if (!converter.ReadMusHeader(reader))
            {
                return false;
            }

            reader.BaseStream.Seek(converter._musHeader.StartPos, SeekOrigin.Begin);

            // Start the output by writing the MIDI header + track header
            using var outputStream = new MemoryStream();
            using var midiWriter = new BinaryWriter(outputStream, Encoding.ASCII, true);

            midiWriter.Write(MidiHeader);
            midiWriter.Write(MidiTrackHeader);

            var scoreEnded = false;

            while (!scoreEnded)
            {
                while (!scoreEnded)
                {
                    // Get the next event by reading the channel number and the event code
                    var musEvent = reader.ReadByte();
                    var channel = converter.GetMidiChannel(musEvent & 0x0F, midiWriter);
                    var musEventType = (MusEventType)(musEvent & 0x70);

                    byte key;
                    byte controllerNumber;
                    switch (musEventType)
                    {
                        case MusEventType.NoteOff:
                            key = reader.ReadByte();
                            converter.WriteNoteOff(channel, key, midiWriter);
                            break;
                        
                        case MusEventType.NoteOn:
                            key = reader.ReadByte();
                            if ((key & 0x80) != 0)
                            {
                                converter._channelVelocities[channel] = (byte)(reader.ReadByte() & 0x7F);
                            }
                            
                            converter.WriteNoteOn(channel, key, midiWriter);
                            break;

                        case MusEventType.PitchBend:
                            key = reader.ReadByte();
                            converter.WritePitchBend(channel, (short)(key * 64), midiWriter);
                            break;

                        case MusEventType.SysEvent:
                            controllerNumber = reader.ReadByte();
                            if (controllerNumber is < 10 or > 14)
                            {
                                return false;
                            }
                            converter.WriteChangeController(channel, CtrlTranslate[controllerNumber], midiWriter);
                            break;

                        case MusEventType.CtrlChange:
                            controllerNumber = reader.ReadByte();
                            var controllerValue = reader.ReadByte();
                            if (controllerNumber == 0)
                            {
                                converter.WriteChangePatch(channel, controllerValue, midiWriter);
                            }
                            else
                            {
                                if (controllerNumber is < 1 or > 9)
                                {
                                    return false;
                                }

                                converter.WriteChangeController(channel, CtrlTranslate[controllerNumber], controllerValue, midiWriter);
                            }
                            break;

                        case MusEventType.ScoreEnd:
                            scoreEnded = true;
                            break;

                        default:
                            return false; // error in MUS file
                    }

                    if ((musEvent & 0x80) != 0)
                    {
                        break;
                    }
                }

                // Now we need to read the time code:
                if (!scoreEnded)
                {
                    uint timeDelay = 0;
                    for (; ; )
                    {
                        var working = reader.ReadByte();
                        timeDelay = (uint)(timeDelay * 128u + (working & 0x7F));
                        if ((working & 0x80) == 0)
                        {
                            break;
                        }
                    }
                    converter._queuedTime += timeDelay;
                }
            }

            converter.WriteEndOfTrack(midiWriter);
            converter.WriteTrackLength(midiWriter);

            midiWriter.Flush();
            midiFile = outputStream.ToArray();
            
            return true;
        }

        private void Initialize()
        {
            Array.Fill(_channelMap, -1);
        }

        private bool ReadMusHeader(BinaryReader reader)
        {
            _musHeader = new MusHeader
            {
                MagicId1 = reader.ReadByte(),
                MagicId2 = reader.ReadByte(),
                MagicId3 = reader.ReadByte(),
                MagicId4 = reader.ReadByte(),
                Length = reader.ReadUInt16(),
                StartPos = reader.ReadUInt16(),
                PrimaryChannels = reader.ReadUInt16(),
                SecondaryChannels = reader.ReadUInt16(),
                InstrumentCount = reader.ReadUInt16()
            };

            return _musHeader.MagicId1 == 'M' &&
                   _musHeader.MagicId2 == 'U' &&
                   _musHeader.MagicId3 == 'S' &&
                   _musHeader.MagicId4 == 0x1A;
        }

        private int GetMidiChannel(int musChannel, BinaryWriter writer)
        {
            if (musChannel == MusPercussionChannel)
            {
                return MidiPercussionChannel;
            }

            if (_channelMap[musChannel] == -1)
            {
                _channelMap[musChannel] = GetFreeChannel();

                // First time using the channel, send an "all notes off" MIDI event
                WriteChangeController(_channelMap[musChannel], 0x7b, writer);
            }

            return _channelMap[musChannel];
        }

        private int GetFreeChannel()
        {
            // Find the current highest-allocated channel.
            var max = -1;

            for (var i = 0; i < NumberOfChannels; ++i)
            {
                if (_channelMap[i] > max)
                {
                    max = _channelMap[i];
                }
            }

            // max is now equal to the highest-allocated MIDI channel.
            // We can now allocate the next available channel.  This also works if no channels are currently allocated (max=-1)
            var result = max + 1;

            // Don't allocate the MIDI percussion channel, skip it!
            if (result == MidiPercussionChannel)
            {
                ++result;
            }

            return result;
        }

        private void WriteNoteOn(int channel, byte key, BinaryWriter writer)
        {
            var working = (byte)((int)MidiEventType.NoteOn | channel);

            WriteTime(_queuedTime, writer);

            writer.Write(working);

            working = (byte)(key & 0x7F);
            writer.Write(working);

            working = (byte)(_channelVelocities[channel] & 0x7F);
            writer.Write(working);

            _trackSize += 3;
        }

        private void WriteNoteOff(int channel, byte key, BinaryWriter writer)
        {
            var working = (byte)((int)MidiEventType.NoteOff | channel);

            WriteTime(_queuedTime, writer);

            writer.Write(working);

            working = (byte)(key & 0x7F);
            writer.Write(working);

            working = 0;
            writer.Write(working);

            _trackSize += 3;
        }

        private void WritePitchBend(int channel, short bend, BinaryWriter writer)
        {
            var working = (byte)((int)MidiEventType.PitchBend | channel);

            WriteTime(_queuedTime, writer);

            writer.Write(working);

            working = (byte)(bend & 0x7F);
            writer.Write(working);

            working = (byte)((bend >> 7) & 0x7F);
            writer.Write(working);

            _trackSize += 3;
        }

        private void WriteChangePatch(int channel, byte patch, BinaryWriter writer)
        {
            var working = (byte)((int)MidiEventType.ProgramChange | channel);

            WriteTime(_queuedTime, writer);

            writer.Write(working);

            working = (byte)(patch & 0x7F);
            writer.Write(working);

            _trackSize += 2;
        }

        private void WriteChangeController(int channel, byte control, BinaryWriter writer)
        {
            WriteChangeController(channel, control, 0, writer);
        }

        private void WriteChangeController(int channel, byte control, byte value, BinaryWriter writer)
        {
            var working = (byte)((int)MidiEventType.CtrlChange | channel);
            
            WriteTime(_queuedTime, writer);
            
            writer.Write(working);

            working = (byte)(control & 0x7F);
            writer.Write(working);

            working = value;
            if ((working & 0x80) != 0)
            {
                working = 0x7F;
            }
            writer.Write(working);

            _trackSize += 3;
        }

        private void WriteEndOfTrack(BinaryWriter writer)
        {
            WriteTime(_queuedTime, writer);

            writer.Write((byte)0xFF);
            writer.Write((byte)0x2F);
            writer.Write((byte)0);

            _trackSize += 3;
        }

        private void WriteTrackLength(BinaryWriter writer)
        {
            writer.BaseStream.Seek(18, SeekOrigin.Begin);
            writer.Write((byte)((_trackSize >> 24) & 0xFF));
            writer.Write((byte)((_trackSize >> 16) & 0xFF));
            writer.Write((byte)((_trackSize >> 8) & 0xFF));
            writer.Write((byte)(_trackSize & 0xFF));
        }

        private void WriteTime(uint time, BinaryWriter writer)
        {
            var buffer = time & 0x7F;

            while ((time >>= 7) != 0)
            {
                buffer <<= 8;
                buffer |= ((time & 0x7F) | 0x80);
            }

            for (;;)
            {
                var writeValue = (byte)(buffer & 0xFF);

                writer.Write(writeValue);
                _trackSize++;

                if ((buffer & 0x80) != 0)
                {
                    buffer >>= 8;
                }
                else
                {
                    _queuedTime = 0;
                    return;
                }
            }
        }

        private struct MusHeader
        {
            public byte MagicId1;
            public byte MagicId2;
            public byte MagicId3;
            public byte MagicId4;
            
            public ushort Length;
            public ushort StartPos;
            public ushort PrimaryChannels;
            public ushort SecondaryChannels;
            public ushort InstrumentCount;
        }
    }
}