﻿using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace Common.Controls
{
    public enum ButtonArrowTypes : byte
    {
        Down,
        Up,
        Left,
        Right
    }

    [ValueConversion(typeof(IsCornerCtrlCorner), typeof(CornerRadius))]
    public class IsCornerCtrlCornerToRadiusConverter : IValueConverter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter">Must be a string that can be converted into an int, either directly as a decimal or as an hexadecimal
        /// prefixed with either 0x or 0X. The first 8 bits will give the CornerRadius rounding next to edge of control. The following bits will
        /// give rounding of a corner not adjoining an edge. Example: 0x305: inner rounding: 0x3, outer rounding: 0x5. Inner rounding of a 
        /// corner not adjoining an edge currently not used and set to 0.</param>
        /// <param name="culture"></param>
        /// <returns></returns>

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                throw new System.ArgumentNullException();

            if (!(value is IsCornerCtrlCorner))
                throw new System.ArgumentException();

            string str = (string)parameter;
            IsCornerCtrlCorner ccc = (IsCornerCtrlCorner)value;

            if (!int.TryParse(str, out int rounding))
            {
                if (!(str[0] == '0' && (str[1] == 'x' || str[1] == 'X')))
                    throw new System.ArgumentException();

                if (!Int32.TryParse(str.Substring(2), System.Globalization.NumberStyles.HexNumber, null, out rounding))
                    throw new System.ArgumentException();
            }
            int NotEdgeRounding = rounding >> 8;
            int EdgeRounding = rounding & 0x000000FF;

            return new CornerRadius(ccc.TopLeft ? EdgeRounding : NotEdgeRounding, ccc.TopRight ? EdgeRounding : NotEdgeRounding,
                                    ccc.BottomRight ? EdgeRounding : NotEdgeRounding, ccc.BottomLeft ? EdgeRounding : NotEdgeRounding);
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException("Not Implemented.");
        }
    }

    public class IsCornerCtrlCornerConverter : TypeConverter
    {
        public IsCornerCtrlCornerConverter() { }
        public override bool CanConvertFrom(ITypeDescriptorContext typeDescriptorContext, Type sourceType)
        {
            return sourceType == Type.GetType("System.String");
        }
        public override bool CanConvertTo(ITypeDescriptorContext typeDescriptorContext, Type destinationType)
        {
            return destinationType == Type.GetType("System.String");
        }
        public override object ConvertFrom(ITypeDescriptorContext typeDescriptorContext, System.Globalization.CultureInfo cultureInfo, object source)
        {
            if (source == null)
                throw new System.ArgumentNullException();

            String[] strArr = ((String)source).Split(',');

            if (strArr.Count() != 4)
                throw new System.ArgumentException();

            bool[] cornerstates = new bool[4];

            for (int i = 0; i < strArr.Count(); i++)
            {
                if (!bool.TryParse(strArr[i], out cornerstates[i]))
                    throw new System.ArgumentException();
            }
            return new IsCornerCtrlCorner(cornerstates[0], cornerstates[1], cornerstates[2], cornerstates[3]);
        }
        public override object ConvertTo(ITypeDescriptorContext typeDescriptorContext, System.Globalization.CultureInfo cultureInfo, object value, Type destinationType)
        {
            if (value == null)
                throw new System.ArgumentNullException();
            if (!(value is IsCornerCtrlCorner))
                throw new System.ArgumentException();

            IsCornerCtrlCorner ccc = (IsCornerCtrlCorner)(value);

            return ccc.TopLeft.ToString() + "," + ccc.TopRight.ToString() + "," + ccc.BottomRight.ToString() + "," + ccc.BottomLeft.ToString();
        }
    }

    /// <summary>
    /// IsCornerCtrlCorner is used to indicate which corners of the arrow button are also on the corner of the container control
    /// in which it is inserted. If for instance the arrow button is placed on right hand side as with a combo box, then both 
    /// right hand sides corners of IsCornerCtrlCorner will be set to true, while both left hand sides will be set to false.
    /// Order is same as with Border.CornerRadius: topleft, topright, bottomright, bottomleft.
    /// Reason for this is because with some themes (example: Aero), button has slightly rounded corners when these are on 
    /// edge of control.
    /// </summary>
    [TypeConverter(typeof(IsCornerCtrlCornerConverter))]
    public struct IsCornerCtrlCorner : IEquatable<IsCornerCtrlCorner>
    {
        private bool topLeft, topRight, bottomRight, bottomLeft;
        public IsCornerCtrlCorner(bool uniformCtrlCorner)
        {
            topLeft = topRight = bottomRight = bottomLeft = uniformCtrlCorner;
        }
        public IsCornerCtrlCorner(bool topLeft, bool topRight, bool bottomRight, bool bottomLeft)
        {
            this.topLeft = topLeft;
            this.topRight = topRight;
            this.bottomRight = bottomRight;
            this.bottomLeft = bottomLeft;
        }
        public bool BottomLeft { get { return bottomLeft; } set { bottomLeft = value; } }
        public bool BottomRight { get { return bottomRight; } set { bottomRight = value; } }
        public bool TopLeft { get { return topLeft; } set { topLeft = value; } }
        public bool TopRight { get { return topRight; } set { topRight = value; } }

        public static bool operator !=(IsCornerCtrlCorner ccc1, IsCornerCtrlCorner ccc2)
        {
            return ccc1.topLeft != ccc2.topLeft || ccc1.topRight != ccc2.topRight ||
                    ccc1.bottomRight != ccc2.bottomRight || ccc1.bottomLeft != ccc2.topLeft;
        }
        public static bool operator ==(IsCornerCtrlCorner ccc1, IsCornerCtrlCorner ccc2)
        {
            return ccc1.topLeft == ccc2.topLeft && ccc1.topRight == ccc2.topRight &&
                ccc1.bottomRight == ccc2.bottomRight && ccc1.bottomLeft == ccc2.topLeft;
        }
        public bool Equals(IsCornerCtrlCorner cornerCtrlCorner)
        {
            return this == cornerCtrlCorner;
        }
        public override bool Equals(object obj)
        {
            if (obj is IsCornerCtrlCorner corner)
                return this == corner;
            else
                return false;
        }
        public override int GetHashCode()
        {
            return (topLeft ? 0x00001000 : 0x00000000) | (topRight ? 0x00000100 : 0x00000000) |
                (bottomRight ? 0x00000010 : 0x00000000) | (bottomLeft ? 0x00000001 : 0x00000000);
        }
        public override string ToString()
        {
            return topLeft.ToString() + "," + topRight.ToString() + "," + bottomRight.ToString() + "," + bottomLeft.ToString();
        }
    }

    public class ArrowRepeatButton : RepeatButton
    {
        public static readonly DependencyProperty ButtonArrowTypeProperty;
        public static readonly DependencyProperty IsCornerCtrlCornerProperty;


        public ButtonArrowTypes ButtonArrowType
        {
            get { return (ButtonArrowTypes)GetValue(ButtonArrowTypeProperty); }
            set { SetValue(ButtonArrowTypeProperty, value); }
        }
        public IsCornerCtrlCorner IsCornerCtrlCorner
        {
            get { return (IsCornerCtrlCorner)GetValue(IsCornerCtrlCornerProperty); }
            set { SetValue(IsCornerCtrlCornerProperty, value); }
        }


        static ArrowRepeatButton()
        {
            Type owner = typeof(ArrowRepeatButton);
            DefaultStyleKeyProperty.OverrideMetadata(owner, new FrameworkPropertyMetadata(owner));
            ButtonArrowTypeProperty = DependencyProperty.Register("ButtonArrowType", typeof(ButtonArrowTypes), owner, new FrameworkPropertyMetadata(ButtonArrowTypes.Down));
            IsCornerCtrlCornerProperty = DependencyProperty.Register("IsCornerCtrlCorner", typeof(IsCornerCtrlCorner), owner, new FrameworkPropertyMetadata(new IsCornerCtrlCorner(false, true, true, false)));
        }
    }
}
