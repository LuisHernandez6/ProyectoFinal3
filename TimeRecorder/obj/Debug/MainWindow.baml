   M S B A M L   `   `   `  ����K  GMonitorActividad, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null3clr-namespace:MonitorActividadMonitorActividad  5   	   U QLiveCharts.Wpf, Version=0.9.7.0, Culture=neutral, PublicKeyToken=0bc1f845d1ebb8dfG4clr-namespace:LiveCharts.Wpf;assembly=LiveCharts.WpfLiveCharts.Wpf 5   	   !    MonitorActividad.MainWindow   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   0localclr-namespace:MonitorActividad     5   	   Dlvc4clr-namespace:LiveCharts.Wpf;assembly=LiveCharts.Wpf     5   	     � Title$  Ventana Principal��5	   	   $	��527��6#   $
��1080��60    � SizeToContent$ Manual=�5
   	   .��5      �  � RowDefinitions 5   
   �� 5      $
��Auto �6   �� 5      $��* �6   .+�5   
   � $��0��6   $��Center=�6   $	��Top=�69   $��10q�6Q    � ColumnDefinitions 5      �� 5      $
��Auto �6#   �� 5      $
��Auto �6#   �� 5      $
��Auto �6#   .+�5      �� -   5         ��Name$ btnPreviousMonth��5      $	��←��6/   $��30��6;   $��0��6F   6V   �� 5      -   5         ��Name$ lblCurrentMonthYear��5      $��abril, 2025��61   $��16(�6G   $��1��6U   $��Center=�6e   $
��10,0q�6�   �� 5      -   5        $ btnNextMonth��5      $	��→��6+   $��30��67   $��2��6B   6R   5      #  LiveCharts.Wpf.CartesianChart  5    
   -   5        $��chartActividad��5       &   LiveCharts.Wpf.Charts.Base.Chart   LegendLocation$ Bottom=�63   $��30,46,30,59q�6K      Series 5"      (  "LiveCharts.Wpf.StackedColumnSeries    LiveCharts.Wpf.Series   Title$ 
Productivo��6*   	   Fill
	 � � �
   Values
 6=   �� *
Productivo+6[     5#      $ Neutral��6*   
	 ��  �
 6:   �� *	Neutral+6U     5$      $ Inactivo��6*   
	 � ���
 6;   �� *
TiempoIdle+6Y     5%      $ No Productivo��6*   
	 �  ��
 6@   �� *NoProductivo+6`     5&      $ No Categorizado��6*   
	 �����
 6B   �� *NoCategorizado+6d      AxisX 5)        LiveCharts.Wpf.Axis     Title$ Días del Mes��6      Labels 60   �� *Dias+   AxisY 5,        $ Horas��6   5.      �� 5/   
   -   5        $ button��5/      $��Cerrar sesion��6!   $��Right=�69   $��26��6U   $��
0,448,22,0q�6a   $	��Top=�6u   $��88��6�   6�   �� 50   
   -   5         ��Name$ comboBox��50      $��Right=�6%   $��	0,12,22,0q�6A   $	��Top=�6T   $	��198��6l   ��6x   �� *Items+6�   �� 51   
   -   5        $ button_Copy��51      $��Eliminar��6&   $��Right=�69   $��26��6U   $��0,448,132,0q�6a   $	��Top=�6v   $��88��6�   6�   �� 52   
   -   5        $ button_Copy1��52      $��	Modificar��6'   $��Right=�6;   $��26��6W   $��0,448,242,0q�6c   $	��Top=�6x   $��88��6�   6�   �� 53   
   -	   5        $ button_Copy2��53      $��Crear usuario��6'   $��Right=�6?   $��26��6[   $��0,448,352,0q�6g   $	��Top=�6|   $��88��6�   6�   �� 54   
   -
   5        $ label��54      $��Label��6   $
��Left=�6/   $��67��6J   $��58,5,0,0q�6V   $	��Top=�6h   $	��352��6�   $
��Bold"�6�   $��19(�6�   �� 55   
   -   5        $ buttonActividad��55      $��Administrar Perfiles��6*   $��Right=�6I   $��26��6e   $��0,448,791,0q�6q   $	��Top=�6�   $	��133��6�   6�   �� 56   
   -   5        $ 
label_Copy��56      $�� ��6$   $��Right=�60   $��22��6L   $��0,449,925,0q�6X   $	��Top=�6m   $��43��6�   57      58      