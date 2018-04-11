# LinkerPleaseIncludeGenerator

a small tool for solving problems with the linker when using MvvmCross

## The problem

The Linker removes all "unneeded" methods and types from the sdk (or even from your assembly) when configured.
But if your project gets larger also your apk file seem to explode (all the sdk stuff you not fully use) - so the linker can help you to shrink your apk file dramatically.

The problem starts when using the Linker and MVVMCross or another MvvM Pattern Framework.
There you only reference Methods and properties from the ui by using it in the MvxBind - so no compiled code references the used properties from the sdk and so the linker removes it.


## The solution

the tool generates a class called LinkerPleaseIncludeGenerated.cs which just uses used properties and types in the code to tell the linker that he should not remove that stuff.

It is created for use with the MvvmCross Framwork as a prebuild-step.

### What it does:

Suggest the following situation:

you have a android layout with an EditText and a floating Save-Button - you bound the Text and the Click with a Command.
Resources/layout/somelayout.axml
```xml
<EditText
    android:hint="please enter some text"
    android:layout_height="wrap_content"
    android:layout_width="match_parent"
    android:inputType="text"
    android:textSize="13sp"
    android:fontFamily="sans-serif-regular"
    local:MvxBind="Text TextProperty" />

<android.support.design.widget.FloatingActionButton
    android:layout_width="wrap_content"
    android:layout_height="wrap_content"
    android:layout_marginBottom="10dp"
    local:backgroundTint="#757575"
    local:srcCompat="@drawable/anmeldung_name_edit_fab"
    local:MvxBind="Click SaveCommand" />
``` 

Everything fine so far - as long as you didnt activate the Linker ;).
Because when you start with setting it to "SDK only" the linker might not find any reference to the EditText.Text Property nor to the 
FloatingActionButton.Click. So he removes them.

when you activate the LinkerPleaseIncludeGenerator he will generate the a class like that (depending on your configuration):

```csharp
using Android.Support.Design.Widget;
using Android.Support.V4.View;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using MvvmCross.Binding.Droid.Views;
using MvvmCross.Droid.Support.V7.RecyclerView;

namespace LinkerPleaseIncludeGenerator
{
    public class LinkerPleaseIncludeMeGeneratedTemplate
    {
        T UseMe<T>(T def) => def;

        void IncludeTextInputEditText(TextInputEditText x) {
            x.Text = UseMe(x.Text);
        }
        void IncludeFloatingActionButton(FloatingActionButton x) {
            x.Click += null;
        }
   }
}
```

now all you need to do is to include this in your project and because the properties are now used (even it makes no sense) it will work now.

## Some details

There is some configuration in the .config file where you can modify the generated code and some other stuff.

As far as the tool does **not reflect** your code it cannot know **custom binding implemetations** of our project and would try to add them in the class, which might cause a compile time error.
please simply add it as ignore in the configuration file.

## Configuration

| property | description |
| --- | --- |
| attributeToSearch | AttributeName to search for bindings default *MvxBind*
| Ignore | A semicolon or newline separated list of propertes or type.property names of things to ignore (e.g. custom binding implementations)
| Events | Properties that should be handled as events default: Click; LongClick
| FilePrefix | class-File Prefix containing the required usings and so on
| TypePrefix | (Method)-Prefix for a found type 
| PropertyTemplate | Template for a property
| EventTemplate | Template for a event
| TypeSuffix | Suffix of a found type/Method (the ending curly braces fot the method)
| FileSuffix | the end of fileSuffix (the ending curly braces for class and namespace)




