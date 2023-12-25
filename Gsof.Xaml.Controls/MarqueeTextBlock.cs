using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Gsof.Xaml.Extensions;

namespace Gsof.Xaml.Controls
{

    // ReSharper disable once StringLiteralTypo
    [TemplatePart(Name = "MARQUEEGRID")]
    public class MarqueeTextBlock : Control
    {
        private const string MarqueeGridName = "MARQUEEGRID";
        private const string MarqueeTextBlockKey = "MARQUEETEXT";
        private FrameworkElement? _marqueeGrid;

        private TextRender? _marqueeTextBlock;
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

        public string? Text
        {
            get => (string?)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Text), typeof(string), typeof(MarqueeTextBlock), new PropertyMetadata("", OnTextPropertyChanged));

        private static void OnTextPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var mtb = dependencyObject as MarqueeTextBlock;
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
            get => (TextTrimming)GetValue(TextTrimmingProperty);
            set => SetValue(TextTrimmingProperty, value);
        }

        // Using a DependencyProperty as the backing store for TextTrimming.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextTrimmingProperty =
            DependencyProperty.Register(nameof(TextTrimming), typeof(TextTrimming), typeof(MarqueeTextBlock), new PropertyMetadata(TextTrimming.CharacterEllipsis));



        public double AnimationTime
        {
            get => (double)GetValue(AnimationTimeProperty);
            set => SetValue(AnimationTimeProperty, value);
        }

        // Using a DependencyProperty as the backing store for AnimationTime.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AnimationTimeProperty =
            DependencyProperty.Register(nameof(AnimationTime), typeof(double), typeof(MarqueeTextBlock), new PropertyMetadata(1500d));

        public bool IsRunAnimation
        {
            get => (bool)GetValue(IsRunAnimationProperty);
            set => SetValue(IsRunAnimationProperty, value);
        }

        // Using a DependencyProperty as the backing store for IsRunRoll.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsRunAnimationProperty =
            DependencyProperty.Register(nameof(IsRunAnimation), typeof(bool), typeof(MarqueeTextBlock), new PropertyMetadata(false, OnIsRunAnimationPropertyChanged));

        public FlowDirection MoveDirection
        {
            get => (FlowDirection)GetValue(MoveDirectionProperty);
            set => SetValue(MoveDirectionProperty, value);
        }

        // Using a DependencyProperty as the backing store for MoveDirection.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MoveDirectionProperty =
            DependencyProperty.Register(nameof(MoveDirection), typeof(FlowDirection), typeof(MarqueeTextBlock), new PropertyMetadata(FlowDirection.RightToLeft));

        private static void OnIsRunAnimationPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            if (dependencyObject is not MarqueeTextBlock)
            {
                return;
            }

            RunAnimation(dependencyObject);
        }

        private static void RunAnimation(DependencyObject dependencyObject)
        {
            if (dependencyObject is not MarqueeTextBlock mtb)
            {
                return;
            }

            mtb.RunAnimation(mtb.IsRunAnimation);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _marqueeGrid = GetTemplateChild(MarqueeGridName) as FrameworkElement;
            _marqueeTextBlock = GetTemplateChild(MarqueeTextBlockKey) as TextRender;

            CreatePathData();
        }

        protected virtual void RunAnimation(bool isStart)
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

            if (_marqueeTextBlock == null)
            {
                return;
            }

            var tt = _marqueeTextBlock.GetRenderTransform<TranslateTransform>();

            tt.BeginAnimation(TranslateTransform.XProperty, null);

            tt.X = 0;

            if (!isStart)
            {
                return;
            }

            var ftc = ConvertFormattedText(this.Text ?? "");
            var textWidth = ftc.WidthIncludingTrailingWhitespace;

            if (textWidth < _marqueeGrid.ActualWidth)
            {
                this.SetCurrentValue(IsRunAnimationProperty, false);
                return;
            }

            _isCreatePath = true;
            CreatePathData(true);

            string text = this.Text + "    ";

            var offset = ConvertFormattedText(text).WidthIncludingTrailingWhitespace;

            var time = TimeSpan.FromMilliseconds(Math.Abs(offset) / 100 * AnimationTime);
            var da = MoveDirection == FlowDirection.RightToLeft ? new DoubleAnimation(-offset, time) : new DoubleAnimation(-offset, 0, time);
            da.RepeatBehavior = RepeatBehavior.Forever;

            tt.BeginAnimation(TranslateTransform.XProperty, da);
        }

        protected virtual void CreatePathData(bool isTwo = false)
        {
            if (_marqueeTextBlock == null || !_isCreatePath || this.Text == null)
            {
                return;
            }

            _isCreatePath = false;

            string text = this.Text;

            if (isTwo)
            {
                text += "    " + this.Text;
            }

            var ft = ConvertFormattedText(text);
            _marqueeTextBlock.FormattedText = ft;

        }

        private void OnMarqueeTextBlockLoaded(object sender, RoutedEventArgs e)
        {
            if (_isRunBeforeInit)
            {
                _isRunBeforeInit = false;
                RunAnimation(IsRunAnimation);
            }
        }

        private FormattedText ConvertFormattedText(string text)
        {
            var typeface = new Typeface(this.FontFamily, this.FontStyle, this.FontWeight, this.FontStretch);

#if NET5_0_OR_GREATER
            var pixelsPerDip = VisualTreeHelper.GetDpi(this).PixelsPerDip;
            return new FormattedText(text, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight,
            typeface, this.FontSize, this.Foreground, pixelsPerDip);
#else
        return new FormattedText(text, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight,
            typeface, this.FontSize, this.Foreground);
#endif


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
}
