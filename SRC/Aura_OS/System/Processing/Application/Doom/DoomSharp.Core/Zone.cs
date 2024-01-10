namespace DoomSharp.Core;

public class Zone
{
    public void Initialize()
    {

    }
    /*
     *
    //
    // Z_Init
    //
    void Z_Init (void)
    {
        memblock_t*	block;
        int		size;

        mainzone = (memzone_t *)I_ZoneBase (&size);
        mainzone->size = size;

        // set the entire zone to one free block
        mainzone->blocklist.next =
	    mainzone->blocklist.prev =
	    block = (memblock_t *)( (byte *)mainzone + sizeof(memzone_t) );

        mainzone->blocklist.user = (void *)mainzone;
        mainzone->blocklist.tag = PU_STATIC;
        mainzone->rover = block;
	    
        block->prev = block->next = &mainzone->blocklist;

        // NULL indicates a free block.
        block->user = NULL;
        
        block->size = mainzone->size - sizeof(memzone_t);
    }
     */
}