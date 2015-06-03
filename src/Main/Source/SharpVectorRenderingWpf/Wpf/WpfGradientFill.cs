using System;
using System.Xml;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

using SharpVectors.Dom.Svg;
using SharpVectors.Dom.Css;
using SharpVectors.Renderers.Utils;

namespace SharpVectors.Renderers.Wpf
{
    public sealed class WpfGradientFill : WpfFill
    {
        #region Private Fields

        private SvgGradientElement _gradientElement;

        #endregion

        #region Constructors and Destructor

        public WpfGradientFill(SvgGradientElement gradientElement)
        {
            _gradientElement = gradientElement;
        }

        #endregion

        #region Public Methods

        public override Brush GetBrush(Rect elementBounds, WpfDrawingContext context)
        {
            SvgLinearGradientElement linearGrad = _gradientElement as SvgLinearGradientElement;
            if (linearGrad != null)
            {
                return GetLinearGradientBrush(elementBounds, linearGrad);
            }

            SvgRadialGradientElement radialGrad = _gradientElement as SvgRadialGradientElement;
            if (radialGrad != null)
            {
                return GetRadialGradientBrush(elementBounds, radialGrad);
            }

            return new SolidColorBrush(Colors.Black);
        }

        #endregion

        #region Private Methods

        private Brush GetLinearGradientBrush(Rect elementBounds,
            SvgLinearGradientElement res)
        {
            double x1 = res.X1.AnimVal.Value;
            double x2 = res.X2.AnimVal.Value;
            double y1 = res.Y1.AnimVal.Value;
            double y2 = res.Y2.AnimVal.Value;

            GradientStopCollection gradientStops = GetGradientStops(res.Stops);

            Brush Brush = null;
            //LinearGradientBrush brush = new LinearGradientBrush(gradientStops);
            var LGbrush = new LinearGradientBrush(gradientStops,
                new Point(x1, y1), new Point(x2, y2));

            SvgSpreadMethod spreadMethod = SvgSpreadMethod.Pad;
            if (res.SpreadMethod != null)
            {
                spreadMethod = (SvgSpreadMethod)res.SpreadMethod.AnimVal;

                if (spreadMethod != SvgSpreadMethod.None)
                {
                    LGbrush.SpreadMethod = WpfConvert.ToSpreadMethod(spreadMethod);
                }
            }


            SvgUnitType mappingMode = SvgUnitType.ObjectBoundingBox;
            if (res.GradientUnits != null)
            {
                mappingMode = (SvgUnitType)res.GradientUnits.AnimVal;
                if (mappingMode == SvgUnitType.ObjectBoundingBox)
                {
                    LGbrush.MappingMode = BrushMappingMode.RelativeToBoundingBox;
                }
                else if (mappingMode == SvgUnitType.UserSpaceOnUse)
                {
                    LGbrush.MappingMode = BrushMappingMode.Absolute;

                }
            }

            string colorInterpolation = res.GetPropertyValue("color-interpolation");
            if (!String.IsNullOrEmpty(colorInterpolation))
            {
                if (colorInterpolation == "linearRGB")
                {
                    LGbrush.ColorInterpolationMode = ColorInterpolationMode.ScRgbLinearInterpolation;
                }
                else
                {
                    LGbrush.ColorInterpolationMode = ColorInterpolationMode.SRgbLinearInterpolation;
                }
            }




            MatrixTransform transform = GetTransformMatrix(res);
            if (transform != null && !transform.Matrix.IsIdentity)
            {
                LGbrush.Transform = transform;

            }
            else
            {
                if (mappingMode == SvgUnitType.ObjectBoundingBox)
                {
                    var drawingBrush = new DrawingBrush();
                    drawingBrush.Stretch = Stretch.Fill;
                    drawingBrush.Viewbox = new Rect(0, 0, 1, 1);
                    var DrawingRect = new GeometryDrawing(LGbrush, null, new RectangleGeometry(new Rect(0, 0, 1, 1)));
                    drawingBrush.Drawing = DrawingRect;
                    Brush = drawingBrush;
                }
            }

            if (Brush == null)
            {
                Brush = LGbrush;
            }

            return Brush;
        }

        private RadialGradientBrush GetRadialGradientBrush(Rect elementBounds,
            SvgRadialGradientElement res)
        {
            double centerX = res.Cx.AnimVal.Value;
            double centerY = res.Cy.AnimVal.Value;
            double focusX = res.Fx.AnimVal.Value;
            double focusY = res.Fy.AnimVal.Value;
            double radius = res.R.AnimVal.Value;

            GradientStopCollection gradientStops = GetGradientStops(res.Stops);

            RadialGradientBrush brush = new RadialGradientBrush(gradientStops);

            brush.RadiusX = radius;
            brush.RadiusY = radius;
            brush.Center = new Point(centerX, centerY);
            brush.GradientOrigin = new Point(focusX, focusY);

            if (res.SpreadMethod != null)
            {
                SvgSpreadMethod spreadMethod = (SvgSpreadMethod)res.SpreadMethod.AnimVal;

                if (spreadMethod != SvgSpreadMethod.None)
                {
                    brush.SpreadMethod = WpfConvert.ToSpreadMethod(spreadMethod);
                }
            }
            if (res.GradientUnits != null)
            {
                SvgUnitType mappingMode = (SvgUnitType)res.GradientUnits.AnimVal;
                if (mappingMode == SvgUnitType.ObjectBoundingBox)
                {
                    brush.MappingMode = BrushMappingMode.RelativeToBoundingBox;
                }
                else if (mappingMode == SvgUnitType.UserSpaceOnUse)
                {
                    brush.MappingMode = BrushMappingMode.Absolute;

                    // use the default value of UserSpaceOnUse mode.
                    // Which is the center of the element.
                    if (res.Fx.AnimVal.UnitType == SvgLengthType.Percentage)
                        brush.GradientOrigin = brush.Center;
                }
            }

            MatrixTransform transform = GetTransformMatrix(res);
            if (transform != null && !transform.Matrix.IsIdentity)
            {
                brush.Transform = transform;
            }
            else
            {
            }

            string colorInterpolation = res.GetPropertyValue("color-interpolation");
            if (!String.IsNullOrEmpty(colorInterpolation))
            {
                if (colorInterpolation == "linearRGB")
                {
                    brush.ColorInterpolationMode = ColorInterpolationMode.SRgbLinearInterpolation;
                }
                else
                {
                    brush.ColorInterpolationMode = ColorInterpolationMode.ScRgbLinearInterpolation;
                }
            }

            return brush;
        }

        private MatrixTransform GetTransformMatrix(SvgGradientElement gradientElement)
        {
            SvgMatrix svgMatrix =
                ((SvgTransformList)gradientElement.GradientTransform.AnimVal).TotalMatrix;

            MatrixTransform transformMatrix = new MatrixTransform(svgMatrix.A, svgMatrix.B, svgMatrix.C,
                svgMatrix.D, svgMatrix.E, svgMatrix.F);

            return transformMatrix;
        }

        private GradientStopCollection GetGradientStops(XmlNodeList stops)
        {
            int itemCount = stops.Count;
            GradientStopCollection gradientStops = new GradientStopCollection(itemCount);

            double lastOffset = 0;
            for (int i = 0; i < itemCount; i++)
            {
                SvgStopElement stop = (SvgStopElement)stops.Item(i);
                string prop = stop.GetAttribute("stop-color");
                string style = stop.GetAttribute("style");
                Color color = Colors.Transparent; // no auto-inherited...
                if (!String.IsNullOrEmpty(prop) || !String.IsNullOrEmpty(style))
                {
                    WpfSvgColor svgColor = new WpfSvgColor(stop, "stop-color");
                    color = svgColor.Color;
                }
                else
                {
                    color = Colors.Black; // the default color...
                    double alpha = 255;
                    string opacity;

                    opacity = stop.GetAttribute("stop-opacity"); // no auto-inherit
                    if (opacity == "inherit") // if explicitly defined...
                    {
                        opacity = stop.GetPropertyValue("stop-opacity");
                    }
                    if (opacity != null && opacity.Length > 0)
                        alpha *= SvgNumber.ParseNumber(opacity);

                    alpha = Math.Min(alpha, 255);
                    alpha = Math.Max(alpha, 0);

                    color = Color.FromArgb((byte)Convert.ToInt32(alpha),
                        color.R, color.G, color.B);
                }

                double offset = stop.Offset.AnimVal;

                offset /= 100;
                offset = Math.Max(lastOffset, offset);

                gradientStops.Add(new GradientStop(color, offset));
                lastOffset = offset;
            }

            if (itemCount == 0)
            {
                gradientStops.Add(new GradientStop(Colors.Black, 0));
                gradientStops.Add(new GradientStop(Colors.Black, 1));
            }

            return gradientStops;
        }

        private Transform FitToViewbox(SvgRect viewBox, Rect rectToFit)
        {
            SvgPreserveAspectRatioType alignment =
                SvgPreserveAspectRatioType.XMidYMid;

            double[] transformArray = FitToViewBox(alignment,
                viewBox,
              new SvgRect(rectToFit.X, rectToFit.Y,
                  rectToFit.Width, rectToFit.Height));

            double translateX = transformArray[0];
            double translateY = transformArray[1];
            double scaleX = transformArray[2];
            double scaleY = transformArray[3];

            Transform translateMatrix = null;
            Transform scaleMatrix = null;
            if (translateX != 0 || translateY != 0)
            {
                translateMatrix = new TranslateTransform(translateX, translateY);
            }
            if ((float)scaleX != 1.0f && (float)scaleY != 1.0f)
            {
                scaleMatrix = new ScaleTransform(scaleX, scaleY);
            }

            if (translateMatrix != null && scaleMatrix != null)
            {
                // Create a TransformGroup to contain the transforms
                // and add the transforms to it.
                TransformGroup transformGroup = new TransformGroup();
                transformGroup.Children.Add(scaleMatrix);
                transformGroup.Children.Add(translateMatrix);

                return transformGroup;
            }
            else if (translateMatrix != null)
            {
                return translateMatrix;
            }
            else if (scaleMatrix != null)
            {
                return scaleMatrix;
            }

            return null;
        }

        private double[] FitToViewBox(SvgPreserveAspectRatioType alignment,
            SvgRect viewBox, SvgRect rectToFit)
        {
            double translateX = 0;
            double translateY = 0;
            double scaleX = 1;
            double scaleY = 1;

            if (!viewBox.IsEmpty)
            {
                // calculate scale values for non-uniform scaling
                scaleX = rectToFit.Width / viewBox.Width;
                scaleY = rectToFit.Height / viewBox.Height;

                if (alignment != SvgPreserveAspectRatioType.None)
                {
                    // uniform scaling
                    scaleX = Math.Max(scaleX, scaleY);

                    scaleY = scaleX;

                    if (alignment == SvgPreserveAspectRatioType.XMidYMax ||
                      alignment == SvgPreserveAspectRatioType.XMidYMid ||
                      alignment == SvgPreserveAspectRatioType.XMidYMin)
                    {
                        // align to the Middle X
                        translateX = (rectToFit.X + rectToFit.Width / 2) - scaleX * (viewBox.X + viewBox.Width / 2);
                    }
                    else if (alignment == SvgPreserveAspectRatioType.XMaxYMax ||
                      alignment == SvgPreserveAspectRatioType.XMaxYMid ||
                      alignment == SvgPreserveAspectRatioType.XMaxYMin)
                    {
                        // align to the right X
                        translateX = (rectToFit.Width - viewBox.Width * scaleX);
                    }

                    if (alignment == SvgPreserveAspectRatioType.XMaxYMid ||
                      alignment == SvgPreserveAspectRatioType.XMidYMid ||
                      alignment == SvgPreserveAspectRatioType.XMinYMid)
                    {
                        // align to the Middle Y
                        translateY = (rectToFit.Y + rectToFit.Height / 2) - scaleY * (viewBox.Y + viewBox.Height / 2);
                    }
                    else if (alignment == SvgPreserveAspectRatioType.XMaxYMax ||
                      alignment == SvgPreserveAspectRatioType.XMidYMax ||
                      alignment == SvgPreserveAspectRatioType.XMinYMax)
                    {
                        // align to the bottom Y
                        translateY = (rectToFit.Height - viewBox.Height * scaleY);
                    }
                }
                else
                {
                    translateX = -viewBox.X * scaleX;
                    translateY = -viewBox.Y * scaleY;
                }
            }

            return new double[]{
                translateX,
                translateY,
                scaleX,
                scaleY };
        }

        #endregion
    }
}
