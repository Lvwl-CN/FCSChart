using FCSChart.Axis;
using FCSChart.Graphical;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace FCSChart.FCSChart
{
    public abstract class FCSChartBase : ChartWithGraphicals
    {
        #region xy轴刻度
        /// <summary>
        /// x轴最大值
        /// </summary>
        public double XMax
        {
            get { return (double)GetValue(XMaxProperty); }
            set { SetValue(XMaxProperty, value); }
        }
        public static readonly DependencyProperty XMaxProperty = DependencyProperty.Register("XMax", typeof(double), typeof(FCSChartBase), new PropertyMetadata(100000d));
        /// <summary>
        /// x轴最小值
        /// </summary>
        public double XMin
        {
            get { return (double)GetValue(XMinProperty); }
            set { SetValue(XMinProperty, value); }
        }
        public static readonly DependencyProperty XMinProperty = DependencyProperty.Register("XMin", typeof(double), typeof(FCSChartBase), new PropertyMetadata(1d));
        /// <summary>
        /// Y轴最大值
        /// </summary>
        public double YMax
        {
            get { return (double)GetValue(YMaxProperty); }
            set { SetValue(YMaxProperty, value); }
        }
        public static readonly DependencyProperty YMaxProperty = DependencyProperty.Register("YMax", typeof(double), typeof(FCSChartBase), new PropertyMetadata(100000d));
        /// <summary>
        /// y轴最小值
        /// </summary>
        public double YMin
        {
            get { return (double)GetValue(YMinProperty); }
            set { SetValue(YMinProperty, value); }
        }
        public static readonly DependencyProperty YMinProperty = DependencyProperty.Register("YMin", typeof(double), typeof(FCSChartBase), new PropertyMetadata(1d));


        /// <summary>
        /// x轴可用的刻度条集合
        /// </summary>
        internal IEnumerable<IAxis> XAxisList
        {
            get { return (IEnumerable<IAxis>)GetValue(XAxisListProperty); }
            set { SetValue(XAxisListProperty, value); }
        }
        internal static readonly DependencyProperty XAxisListProperty = DependencyProperty.Register("XAxisList", typeof(IEnumerable<IAxis>), typeof(FCSChartBase), new PropertyMetadata(null));

        /// <summary>
        /// y轴可用的刻度条集合
        /// </summary>
        internal IEnumerable<IAxis> YAxisList
        {
            get { return (IEnumerable<IAxis>)GetValue(YAxisListProperty); }
            set { SetValue(YAxisListProperty, value); }
        }
        internal static readonly DependencyProperty YAxisListProperty = DependencyProperty.Register("YAxisList", typeof(IEnumerable<IAxis>), typeof(FCSChartBase), new PropertyMetadata(null));
        /// <summary>
        /// 填充x轴刻度条选项
        /// </summary>
        protected virtual void FillXAxisList()
        {
            var line = new LinearNumberAxis();
            var log = new LogarithmicNumberAxis();
            var logbiex = new LogicleBiexAxis();
            Binding maxbinding = new Binding("XMax") { Mode = BindingMode.TwoWay };
            Binding minbinding = new Binding("XMin") { Mode = BindingMode.TwoWay };
            minbinding.Source = maxbinding.Source = this;
            line.SetBinding(IAxis.MaxProperty, maxbinding);
            line.SetBinding(IAxis.MinProperty, minbinding);
            log.SetBinding(IAxis.MaxProperty, maxbinding);
            log.SetBinding(IAxis.MinProperty, minbinding);
            logbiex.SetBinding(IAxis.MaxProperty, maxbinding);
            logbiex.SetBinding(IAxis.MinProperty, minbinding);
            XAxisList = new List<IAxis>() { line, log, logbiex };
            XAxis = line;
        }
        /// <summary>
        /// 填充y轴刻度条选项
        /// </summary>
        protected virtual void FillYAxisList()
        {
            var line = new LinearNumberAxis();
            var log = new LogarithmicNumberAxis();
            var logbiex = new LogicleBiexAxis();
            Binding maxbinding = new Binding("YMax") { Mode = BindingMode.TwoWay };
            Binding minbinding = new Binding("YMin") { Mode = BindingMode.TwoWay };
            minbinding.Source = maxbinding.Source = this;
            line.SetBinding(IAxis.MaxProperty, maxbinding);
            line.SetBinding(IAxis.MinProperty, minbinding);
            log.SetBinding(IAxis.MaxProperty, maxbinding);
            log.SetBinding(IAxis.MinProperty, minbinding);
            logbiex.SetBinding(IAxis.MaxProperty, maxbinding);
            logbiex.SetBinding(IAxis.MinProperty, minbinding);
            YAxisList = new List<IAxis>() { line, log, logbiex };
            YAxis = line;
        }
        #endregion

        #region fcs参数
        /// <summary>
        /// FCS参数集合
        /// </summary>
        public IEnumerable Parameters
        {
            get { return (IEnumerable)GetValue(ParametersProperty); }
            set { SetValue(ParametersProperty, value); }
        }
        public static readonly DependencyProperty ParametersProperty = DependencyProperty.Register("Parameters", typeof(IEnumerable), typeof(FCSChartBase), new PropertyMetadata(null));

        /// <summary>
        /// x轴参数数据模板
        /// </summary>
        public DataTemplate XParamDataTemplate
        {
            get { return (DataTemplate)GetValue(XParamDataTemplateProperty); }
            set { SetValue(XParamDataTemplateProperty, value); }
        }
        public static readonly DependencyProperty XParamDataTemplateProperty = DependencyProperty.Register("XParamDataTemplate", typeof(DataTemplate), typeof(FCSChartBase), new PropertyMetadata(null));

        /// <summary>
        /// y轴参数数据模板
        /// </summary>
        public DataTemplate YParamDataTemplate
        {
            get { return (DataTemplate)GetValue(YParamDataTemplateProperty); }
            set { SetValue(YParamDataTemplateProperty, value); }
        }
        public static readonly DependencyProperty YParamDataTemplateProperty = DependencyProperty.Register("YParamDataTemplate", typeof(DataTemplate), typeof(FCSChartBase), new PropertyMetadata(null));
        #endregion

        #region 画门
        /// <summary>
        /// 多边形绘制命令
        /// </summary>
        public ICommand PolygonCommand
        {
            get { return (ICommand)GetValue(PolygonCommandProperty); }
            set { SetValue(PolygonCommandProperty, value); }
        }
        public static readonly DependencyProperty PolygonCommandProperty = DependencyProperty.Register("PolygonCommand", typeof(ICommand), typeof(FCSChartBase), new PropertyMetadata(null));

        /// <summary>
        /// 四边边形绘制命令
        /// </summary>
        public ICommand QuadrilateralCommand
        {
            get { return (ICommand)GetValue(QuadrilateralCommandProperty); }
            set { SetValue(QuadrilateralCommandProperty, value); }
        }
        public static readonly DependencyProperty QuadrilateralCommandProperty = DependencyProperty.Register("QuadrilateralCommand", typeof(ICommand), typeof(FCSChartBase), new PropertyMetadata(null));

        /// <summary>
        /// 段选门绘制命令
        /// </summary>
        public ICommand VerticalCommand
        {
            get { return (ICommand)GetValue(VerticalCommandProperty); }
            set { SetValue(VerticalCommandProperty, value); }
        }
        public static readonly DependencyProperty VerticalCommandProperty = DependencyProperty.Register("VerticalCommand", typeof(ICommand), typeof(FCSChartBase), new PropertyMetadata(null));

        protected virtual void FillGraphicalCommand()
        {
            PolygonCommand = new DelegateCommand((e) => { Graphicals.Add(new PolygonGraphical()); });
            QuadrilateralCommand = new DelegateCommand((e) => { Graphicals.Add(new QuadrilateralGraphical()); });
            VerticalCommand = new DelegateCommand((e) => { Graphicals.Add(new VerticalGraphical()); });
        }

        #endregion

        public FCSChartBase()
        {
            FillXAxisList();
            FillYAxisList();
            FillGraphicalCommand();
        }




    }
}
