   M S B A M L   `   `   `  ���� G  CTimeRecorder, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null+clr-namespace:TimeRecorderTimeRecorder      TimeRecorder.MainWindow   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     ,localclr-namespace:TimeRecorder     -     � Title$  Time Recorder��$	��500��$
��1080�� � DataContext    TimeRecorder.Processes  .���  � RowDefinitions �� $��10 ��� $��40 ��� $��* ��� $
��auto � � ColumnDefinitions �� $��10 ��� $��* ��� $��10 �.+�&   System.Windows.Controls.DataGrid  -     Name$ ProcessViewList��$��2��$��1��$��14(�   AutoGenerateColumns .     CanUserAddRows .     RowHeaderWidth$ 0Q����� *ProcessList+   	CellStyle �� 	 �� 
TargetType*  $System.Windows.Controls.DataGridCell"	  .��� 
 �� Property
 �@J�IsHitTestVisible �� Value .  �� 
 � ��$ Center=��� 
 � ��$ 2,2,2,2q�   Columns 4  .System.Windows.Controls.DataGridTemplateColumn  0  *System.Windows.Controls.DataGridTextColumn  ,  &System.Windows.Controls.DataGridColumn   Header$ Program Name��   FontSize$ 15Q�1  +System.Windows.Controls.DataGridBoundColumn   Binding �� *Name+  $ Process Name��$ 15Q�   Width5  /System.Windows.Controls.DataGridLengthConverter$	 120  �� *PName+  $ Window Title��$ 15Q�$	 120  �� *	WndName+  $ Total Hours��$ 15Q� �� *	ViewHours+ �� StringFormat$
 0.00��  $ 	Minimized��$ 15Q� �� *
ViewMinH+$
 0.00��  $ Focused��$ 15Q� �� *
ViewFocusH+$
 0.00��  $ +Inputs��$ 15Q� �� *
ViewInputH+$
 0.00��  $ K��$ 15Q� �� *ViewInputKeyH+$
 0.00��   HeaderStyle �� =	  7System.Windows.Controls.Primitives.DataGridColumnHeader"	 	 .��� 
 � ��$ Center=�  $ M��$ 15Q� �� *ViewInputMouseH+$
 0.00�� �� "	 	 .��� 
 � ��$ Center=�  $	 K+M��$ 15Q� �� *ViewInputKMH+$
 0.00�� �� "	 	 .��� 
 � ��$ Center=�  $ C��$ 15Q� �� *ViewInputJoyH+$
 0.00�� �� "	 	 .��� 
 � ��$ Center=�  $ First Recorded Date��$ 15Q� �� *First+  $ Last Recorded Date��$ 15Q� �� *Last+�� $��1��$��1��$��15(�$E��?Records and saves the running time of the elements on the list.���� $��3��$��1��$��10(�$��Right=�$#��ver 1.0.2  By DottierGalaxy50��
��������� -    ��Name$ 	AddButton��$��1��$��1��$	��100��$��25��$��Right=�$��Add Program��