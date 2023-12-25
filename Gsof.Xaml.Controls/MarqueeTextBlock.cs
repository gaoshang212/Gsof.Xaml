using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Gsof.Xaml.Extensions;

namespace Gsof.Xaml.Controls
{

    [TemplatePart(Name = "MARQUEEGRID")]
    public class MarqueeTextBlock : Control
    {
        private bool _hasApplyTemplate = false;

        private const string MarqueeGridName = "MARQUEEGRID";
        private const string DisplayTextblock = "DISPLAYTEXT";
        private const string MarqueeTextblock = "MARQUEETEXT";
        private FrameworkElement _marqueeGrid;
        private TextBlock _displayTextblock;
        //private Path _marqueeTextblock;
        private TextRender? _marqueeTextblock;
        private TextTrimming _cacheTextTrimming;
        private bool _isRunBeforeInit;

        private bool _isCreatePath;

        static MarqueeTextBlock()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MarqueeTextBlock), new FrameworkPropertyMetadata(typeof(MarqueeTextBlock)));
        }

        public MarqueeTextBlock()
        {
            this.Loaded += OnMarqueeTextBlockLoaded;
        }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(MarqueeTextBlock), new PropertyMetadata("", OnTextPropertyChanged));

        private static void OnTextPropertyChanged(DependencyObject p_dependencyObject, DependencyPropertyChangedEventArgs p_dependencyPropertyChangedEventArgs)
        {
            var mtb = p_dependencyObject as MarqueeTextBlock;
            if (mtb == null)
            {
                return;
            }

            mtb._isCreatePath = true;

            if (mtb._isRunBeforeInit)
            {
                mtb.CreatePathData();
                mtb.RunAnimation(mtb.IsRunAnimation);
            }
        }


        public TextTrimming TextTrimming
        {
            get { return (TextTrimming)GetValue(TextTrimmingProperty); }
            set { SetValue(TextTrimmingProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TextTrimming.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextTrimmingProperty =
            DependencyProperty.Register("TextTrimming", typeof(TextTrimming), typeof(MarqueeTextBlock), new PropertyMetadata(TextTrimming.CharacterEllipsis));



        public double AnimationTime
        {
            get { return (double)GetValue(AnimationTimeProperty); }
            set { SetValue(AnimationTimeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AnimationTime.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AnimationTimeProperty =
            DependencyProperty.Register("AnimationTime", typeof(double), typeof(MarqueeTextBlock), new PropertyMetadata(1500d));

        public bool IsRunAnimation
        {
            get { return (bool)GetValue(IsRunAnimationProperty); }
            set { SetValue(IsRunAnimationProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsRunRoll.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsRunAnimationProperty =
            DependencyProperty.Register("IsRunAnimation", typeof(bool), typeof(MarqueeTextBlock), new PropertyMetadata(false, OnIsRunAnimationPropertyChanged));

        public FlowDirection MoveDirection
        {
            get { return (FlowDirection)GetValue(MoveDirectionProperty); }
            set { SetValue(MoveDirectionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MoveDirection.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MoveDirectionProperty =
            DependencyProperty.Register("MoveDirection", typeof(FlowDirection), typeof(MarqueeTextBlock), new PropertyMetadata(FlowDirection.RightToLeft));

        private static void OnIsRunAnimationPropertyChanged(DependencyObject p_dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var mtb = p_dependencyObject as MarqueeTextBlock;
            if (mtb == null)
            {
                return;
            }

            //if (mtb.IsRunAnimation)
            //{
            //    mtb._cacheTextTrimming = mtb.TextTrimming;
            //    mtb.TextTrimming = TextTrimming.None;
            //}
            //else
            //{
            //    mtb.TextTrimming = mtb._cacheTextTrimming;
            //    mtb._cacheTextTrimming = TextTrimming.None;
            //}

            RunAnimation(p_dependencyObject, e);
        }

        private static void RunAnimation(DependencyObject p_dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var mtb = p_dependencyObject as MarqueeTextBlock;
            if (mtb == null)
            {
                return;
            }

            mtb.RunAnimation(mtb.IsRunAnimation);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _marqueeGrid = GetTemplateChild(MarqueeGridName) as FrameworkElement;
            _displayTextblock = GetTemplateChild(DisplayTextblock) as TextBlock;
            //_marqueeTextblock = GetTemplateChild(MarqueeTextblock) as Path;
            _marqueeTextblock = GetTemplateChild(MarqueeTextblock) as TextRender;

            CreatePathData();

            _hasApplyTemplate = true;
        }

        protected virtual void RunAnimation(bool p_isStart)
        {
            if (_marqueeGrid == null)
            {
                _isRunBeforeInit = true;
                return;
            }

            if (Math.Abs(_marqueeGrid.ActualWidth) < double.Epsilon)
            {
                return;
            }


            var tt = _marqueeTextblock.GetRenderTransform<TranslateTransform>();

            tt.BeginAnimation(TranslateTransform.XProperty, null);

            tt.X = 0;

            if (p_isStart)
            {
                var ftc = FormattedTextConverter.ConvertFormattedText(this.Text, this.FontFamily, this.FontSize, this.Foreground);
                var textWidth = ftc.WidthIncludingTrailingWhitespace;

                if (textWidth < this.ActualHeight)
                {
                    this.SetCurrentValue(IsRunAnimationProperty, false);
                    return;
                }

                _isCreatePath = true;
                CreatePathData(true);

                string text = this.Text + "    ";

                var offset = FormattedTextConverter.ConvertFormattedText(text, this.FontFamily, this.FontSize, this.Foreground).WidthIncludingTrailingWhitespace;

                var time = TimeSpan.FromMilliseconds(Math.Abs(offset) / 100 * AnimationTime);
                var da = MoveDirection == FlowDirection.RightToLeft ? new DoubleAnimation(-offset, time) : new DoubleAnimation(-offset, 0, time);
                da.RepeatBehavior = RepeatBehavior.Forever;

                tt.BeginAnimation(TranslateTransform.XProperty, da);
            }
        }

        protected virtual void CreatePathData(bool p_isTwo = false)
        {
            if (_marqueeTextblock == null || !_isCreatePath || this.Text == null)
            {
                return;
            }

            _isCreatePath = false;

            string text = this.Text;

            if (p_isTwo)
            {
                text += "    " + this.Text;
            }

            //_marqueeTextblock.Data = FormattedTextConverter.ConvertFormattedText(text, this.FontFamily,
            //    this.FontSize, this.Foreground).BuildGeometry(new Point(0, 0));

            var ft = ConvertFormattedText(text);

            _marqueeTextblock.FormattedText = ft;
            //ft.BuildGeometry(new Point(0, 0)).GetFlattenedPathGeometry();

        }

        private void OnMarqueeTextBlockLoaded(object sender, RoutedEventArgs e)
        {
            if (_isRunBeforeInit)
            {
                _isRunBeforeInit = false;
                RunAnimation(IsRunAnimation);
            }
        }

        private FormattedText ConvertFormattedText(string p_string)
        {
            Typeface typeface = new Typeface(this.FontFamily, this.FontStyle, this.FontWeight, this.FontStretch);

            return new FormattedText(p_string, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight,
                typeface, this.FontSize, this.Foreground);
        }

    }

    public class TextRender : FrameworkElement
    {
        public FormattedText? FormattedText
        {
            get => (FormattedText?)GetValue(FormattedTextProperty);
            set => SetValue(FormattedTextProperty, value);
        }

        // Using a DependencyProperty as the backing store for FormattedText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FormattedTextProperty =
            DependencyProperty.Register(nameof(FormattedText), typeof(FormattedText), typeof(TextRender), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));


        protected override void OnRender(DrawingContext drawingContext)
        {
            if (this.FormattedText == null)
            {
                return;
            }

            drawingContext.DrawText(FormattedText, new Point(0, 0));
        }
    }

    public class FormattedTextConverter : DependencyObject, IValueConverter
    {
        public FontFamily FontFamily
        {
            get => (FontFamily)GetValue(FontFamilyProperty);
            set { SetValue(FontFamilyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FontFamily.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FontFamilyProperty =
            DependencyProperty.Register("FontFamily", typeof(FontFamily), typeof(FormattedTextConverter), new PropertyMetadata(new FontFamily()));

        public double TextBlockHeight
        {
            get { return (double)GetValue(TextBlockHeightProperty); }
            set { SetValue(TextBlockHeightProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TextBlockHeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextBlockHeightProperty =
            DependencyProperty.Register("TextBlockHeight", typeof(double), typeof(FormattedTextConverter), new PropertyMetadata(0d));



        public double FontSize
        {
            get { return (double)GetValue(FontSizeProperty); }
            set { SetValue(FontSizeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FontSize.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FontSizeProperty =
            DependencyProperty.Register("FontSize", typeof(double), typeof(FormattedTextConverter));




        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
            {
                return string.Empty;
            }

            double fontSize = 12D;

            if (parameter != null)
            {
                double.TryParse(parameter.ToString(), out fontSize);
            }

            return ConvertFormattedText(value.ToString(), FontFamily, fontSize, Brushes.Black).BuildGeometry(new Point(5, 0));
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public static FormattedText ConvertFormattedText(string p_string, string p_fontFamily, double p_fontSize, Color p_color)
        {
            return ConvertFormattedText(p_string, new FontFamily(p_fontFamily), p_fontSize, new SolidColorBrush(p_color));
        }

        public static FormattedText ConvertFormattedText(string p_string, FontFamily p_fontFamily, double p_fontSize,
            Brush p_brushe)
        {
            Typeface typeface = new Typeface(p_fontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);

            return new FormattedText(p_string, CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                typeface, p_fontSize, p_brushe);
        }
    }
}
