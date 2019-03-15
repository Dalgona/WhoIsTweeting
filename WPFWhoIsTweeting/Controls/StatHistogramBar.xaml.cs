using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Wit.Controls
{
    public partial class StatHistogramBar : UserControl
    {
        public static readonly DependencyProperty OnlineBrushProperty =
            DependencyProperty.Register(
                nameof(OnlineBrush),
                typeof(Brush),
                typeof(StatHistogramBar),
                new FrameworkPropertyMetadata(Brushes.Green, FrameworkPropertyMetadataOptions.AffectsRender)
            );

        public static readonly DependencyProperty AwayBrushProperty =
            DependencyProperty.Register(
                nameof(AwayBrush),
                typeof(Brush),
                typeof(StatHistogramBar),
                new FrameworkPropertyMetadata(Brushes.Gray, FrameworkPropertyMetadataOptions.AffectsRender)
            );

        public Brush OnlineBrush
        {
            get => (Brush)GetValue(OnlineBrushProperty);
            set => SetValue(OnlineBrushProperty, value);
        }

        public Brush AwayBrush
        {
            get => (Brush)GetValue(AwayBrushProperty);
            set => SetValue(AwayBrushProperty, value);
        }

        public StatHistogramBar()
        {
            InitializeComponent();
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.Property == DataContextProperty)
            {
                if (DataContext is KeyValuePair<DateTime, int[]> item)
                {
                    row00.Height = new GridLength(item.Value[2], GridUnitType.Star);
                    row01.Height = new GridLength(item.Value[0] + item.Value[1], GridUnitType.Star);
                    row10.Height = new GridLength(item.Value[1], GridUnitType.Star);
                    row11.Height = new GridLength(item.Value[0], GridUnitType.Star);
                }
            }
        }
    }
}
