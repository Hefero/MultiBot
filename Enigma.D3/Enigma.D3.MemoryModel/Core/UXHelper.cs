using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Enigma.D3.MemoryModel.Controls;

namespace Enigma.D3.MemoryModel.Core
{
    public class UXHelper
    {
        public static UXControl GetControl(string name)
        {
            return UXControl.getByName(name);
        }

        public static T GetControl<T>(string name) where T : UXControl
        {
            return GetIfNotNull(
                GetControlPointer(name),
                ptr => ptr.Cast<T>().Dereference());
        }

        public static Memory.Ptr GetControlPointer(string name)
        {
            return GetIfNotNull(MemoryContext.Current.DataSegment.ObjectManager.UIManager.PtrControlsMap.Dereference(), map => map[name]);
        }

        private static TResult GetIfNotNull<T, TResult>(T input, Func<T, TResult> getter)
        {
            if (input == null)
                return default(TResult);
            return getter.Invoke(input);
        }

        public static string GetLastClickedControlName()
        {
            //see https://github.com/Enigma32/Enigma.D3/blob/legacy/Enigma.D3.Memory/D3/UI/UIManager.cs
            string c = MemoryContext.Current.DataSegment.ObjectManager.UIManager.PlatformRead<UIID>(0x0828, 0x0830).Name;
            if (c != null && (c.Equals("None") || c.Length == 0))
                return null;
            else
                return c;
        }

        public static string GetMouseOverControlName()
        {
            //see https://github.com/Enigma32/Enigma.D3/blob/legacy/Enigma.D3.Memory/D3/UI/UIManager.cs
            string c = MemoryContext.Current.DataSegment.ObjectManager.UIManager.PlatformRead<UIID>(0x0A30, 0x0A38).Name;
            if (c != null && (c.Equals("None") || c.Length == 0))
                return null;
            else
                return c;
        }
        public static List<Control> ListAllControls()
        {
            var controls = MemoryContext.Current.DataSegment.ObjectManager.UIManager.PtrControlsMap.Dereference().Select(x => x.Value.Dereference()).ToList();
            return controls;
        }

        //Mutliplatform UXControl
        public class UXControl : Memory.MemoryObject
        {
            private int flags_addr = SymbolTable.Current.Platform == Platform.X86 ? 0x014 : 0x014 + 4;
            private int uirect_addr = SymbolTable.Current.Platform == Platform.X86 ? 0x468 : 0x468 + 0x30;

            private int label_text_addr = SymbolTable.Current.Platform == Platform.X86 ? 0xA20 : 0xA20 + 0x38;
            private int label_text_length_addr = SymbolTable.Current.Platform == Platform.X86 ? 0xA20 + 0x10 : 0xA20 + 0x10 + 0x48;

            public int flags { get { return Read<int>(flags_addr); } } // 1 = IsValid?, 2 = ???, 3 = IsVisible?

            public UIRect uirect
            {
                get
                {
                    return Read<UIRect>(uirect_addr);
                }
            }

            public string label_text
            {
                get
                {
                    return ReadStringPointer(label_text_addr, label_text_length).Dereference();
                }
            }
            public int label_text_length
            {
                get
                {
                    return Read<int>(label_text_length_addr);
                }
            }



            public bool IsVisible()
            {
                return (flags & 4) != 0;
            }

            public static UXControl getByName(string controlname)
            {
                return GetControl<UXControl>(controlname);
            }
            public struct UIRect
            {
                public float Left;
                public float Top;
                public float Right;
                public float Bottom;

                public float Width { get { return Right - Left; } }
                public float Height { get { return Bottom - Top; } }

                public override string ToString()
                {
                    return string.Format("{0} x {1} - L: {2}, T: {3}, R: {4}, B: {5}", Width, Height, Left, Top, Right, Bottom);
                }

                /// <summary>
                /// This is just an example, change type to fit whatever UI system you're using for rendering.
                /// WinForm uses pixels (int) while WPF uses DPI (float).
                /// </summary>
                public UIRect TranslateToClientRect(float clientWidth, float clientHeight)
                {
                    // All sizes/positions are based on a Root of height 1200. Its width will depend on
                    // what aspect ratio the window (client) currently has.
                    const float normalizedHeight = 1200.0f;

                    var aspectRatio = clientWidth / clientHeight;
                    var uiScale = clientHeight / normalizedHeight;
                    var uiRootWidth = aspectRatio * normalizedHeight;
                    var uiRootHeight = normalizedHeight;

                    // An offset is applied to the horizontal axis in order to compensate for aspect ratio
                    // changes. You can see the base is UIRoot width for 4:3 ratio.
                    var horizontalOffset = (float)((int)((uiRootHeight * 4f / 3f) - uiRootWidth) >> 1);

                    var clientRect = new UIRect();
                    clientRect.Left = (Left - horizontalOffset) * uiScale;
                    clientRect.Top = (Top * uiScale);
                    clientRect.Right = clientRect.Left + (Width * uiScale);
                    clientRect.Bottom = clientRect.Top + (Height * uiScale);
                    return clientRect;
                }
            }

        }
    }
}
