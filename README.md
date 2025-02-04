# VrcLeash

This is a combination module for VRCOSC and Unity component that allows you to add pullable functionality to VRC leashes.

If you're looking for OSC Leash compatibility, check out either [CrookedToe's VRCOSC Module](https://github.com/CrookedToe/CrookedToe-s-Modules) or the original [Zenith's Original OSCLeash](https://github.com/ZenithVal/OSCLeash).

If there's interest, I may add backwards compatibility in the future, but otherwise I recommend the above. Especially CrookedToe's for convenience and vertical playspace support.

## Setup

VrcLeash requires two parts to work. First, you need to add the necessary Unity component to your Avatar's leash. This adds the necessary parameters and contacts that allow VrcLeash to track the position and pull of your leash.
Then, you need to set the VRCOSC module. This tracks the leash in game, and communicates back VRChat the necessary movements.

### Setting up the Leash in UNity

Any leash object that utilizes a Physbone is compatible with this component.

To add VrcLeash to you leash, drop the `VrcLeash.unitypackage` into your project. Then add the Vrc Leash component to the root of your leash object. Clicking "Auto Setup" will default the leash name to Leash, and automatically locate the Physbone for the leash and wire up the necessary contacts to make the leash work.

You'll want to note, and possible modify the leash stretch value. You'll move depending on how much the leash is "stretched", so you'll want to make sure the Leash is stretchable and that the value of the stretch is set to something that looks good when fully stretched.

### Setting up VRCOSC

The external logic is powered by the [VRCOSC](https://github.com/VolcanicArts/VRCOSC) toolkit. Download and install that program, and then install the VRC Pullable Leash module. Then going into the `VRC Leash` settings and add a Leash. This will default to to mostly sane values, and you'll want to modify these to find what feels right. You should set the Running and Walking Deadzone values based on the values from the Debug info of the Unity Component above, as these control when the leash's "stretch" will start pulling you.

Then it should all work!
