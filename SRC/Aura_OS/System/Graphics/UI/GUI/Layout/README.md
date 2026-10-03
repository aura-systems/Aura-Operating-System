# App layout files

An app's window and controls are described in `Resources/UI/Layouts/<Name>.xml` (embedded in the
kernel). The app class keeps only the behaviour: it finds its controls by `id` and gives the code of
the events named in the file.

```xml
<Window title="Hello" width="300" height="140" icon="16-program.bmp" padding="6" spacing="10">
  <Grid columns="auto,*" columnSpacing="8" rowHeight="23">
    <Row>
      <Label text="Name:" />
      <TextBox id="name" onEnter="greet" />
    </Row>
  </Grid>
  <Button text="Greet" align="center" onClick="greet" />
  <Label id="greeting" align="center" color="green" />
</Window>
```

```csharp
public class HelloApp : Application
{
    private TextBox _name;
    private Label _greeting;

    public HelloApp(int x = 0, int y = 0) : base(AppLayout.Load("Hello"), x, y)
    {
        _name = Find<TextBox>("name");
        _greeting = Find<Label>("greeting");

        On("greet", () => _greeting.Text = "Hello " + _name.Text + "!");
    }
}
```

Register it with `RegisterApplication(typeof(HelloApp), x, y)` (the file gives the size) and create it
with `new HelloApp(config.X, config.Y)` in `ApplicationManager.Instantiate`. `SetTitle` changes the
title the file gives (the Editor shows the file path).

`Application` updates, places and draws the controls: the app overrides `Update` or `Draw` only for
work of its own, and calls the base method. A wrong file throws an `InvalidDataException` naming it
when the app opens; an event with no handler is logged when it fires.

## Window

The root element. Its elements are stacked top to bottom.

| Attribute | |
|---|---|
| `title` | Window title and process name. |
| `width`, `height` | Window size. |
| `icon` | Icon key (`16-settings.bmp`), default the program icon. |
| `padding` | Space inside the borders, under the title bar. |
| `spacing` | Space between the stacked elements. |

## Containers

- `Stack`: its elements one after the other. `orientation="vertical|horizontal"`, `spacing`, `padding`.
  An element sized `*` along the stack takes the space the others leave.
- `Panel`: a `Stack` drawn on a colored background that fills its space, like a toolbar.
  `color` (default `#DFDFDF`), `borders="true"` for a sunken border, and the `Stack` attributes.
- `Grid`: aligned columns, like a form. `columns="auto,*,120"` (widest element, the space left,
  pixels), `columnSpacing`, `rowSpacing`, `rowHeight` (minimum), `padding`. It holds `Row` elements,
  whose elements fill the columns from the left; hiding a `Row` hides the line.

## Controls

| Element | Attributes | Events |
|---|---|---|
| `Label` | `text`, `color` | |
| `Image` | `src` (embedded image, `UI/Images/AuraLogo.bmp`) or `icon` (icon key, `32-folder.bmp`) | |
| `Button` | `text`, `icon` | `onClick` |
| `TextBox` | `text`, `multiline`, `password` | `onEnter` |
| `Checkbox` | `text`, `color`, `checked` | `onChange` |
| `Slider` | `value` (0 to 255) | `onChange` |
| `DropDown` | `selectedIndex`; `<Item>text</Item>` children | `onChange` |
| `Console` | `cursor` (draws the cursor and the line being typed, `Console.Input`), `scrollBar` (a vertical scroll bar on the right once lines scrolled off the top; its width is kept from the start); default 400 x 300 | |
| `Dialog` | `title`, `message`, `state="information|error"`; `<Button text onClick>` children | |

A `Dialog` is a child of the window only. It sits centered over the window and, while visible, is the
only control that takes input. It usually starts with `visible="false"`.

## Common attributes

| Attribute | |
|---|---|
| `id` | Name for `Find<T>(id)` and `Layout.SetVisible(id, visible)`. |
| `visible` | `false` hides the element; it takes no space, the next elements move up. |
| `width`, `height` | Pixels, `auto` or `*` (fill). Fields (`TextBox`, `Slider`, `DropDown`) default to 200 x 23 and fill their space; other controls fit their content (a label its text, an image its bitmap). |
| `margin` | Space around the element. |
| `align` | `left`, `center`, `right` or `stretch` in the element's space. |
| `valign` | `top`, `center` (default) or `bottom`, `stretch`. |
| `columnSpan` | Grid columns the element takes. |
| `x`, `y` | Position in the window's content area, out of the stack (window elements only). |

Margins and paddings are `4` (all sides), `4,2` (left and right, top and bottom) or `4,2,4,2` (left,
top, right, bottom). Colors are `#RRGGBB`, `#AARRGGBB` or `black`, `white`, `gray`, `darkgray`,
`lightgray`, `red`, `green`, `blue`, `transparent`. `&amp;`, `&lt;`, `&gt;`, `&quot;` and `&apos;`
write those characters.

The elements are placed again when the window is resized, and when the app's code shows or hides a
control (`Visible`, `Layout.SetVisible`) or changes a label's text or a picture's image: the others
follow. Two controls in a `Stack` of fixed height, one of them hidden, take turns in the same place
(SystemInfo's update button and result).
