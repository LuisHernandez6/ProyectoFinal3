   M S B A M L   `   `   `  ����G  CTimeRecorder, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null+clr-namespace:TimeRecorderTimeRecorder  5   	       TimeRecorder.MainWindow   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   ,localclr-namespace:TimeRecorder     5   	   -   5          � Title$  Time Recorder��5	   	   $	��500��6   $
��1080��6,    � DataContext 5
         TimeRecorder.Processes  5   
   .��5      �  � RowDefinitions 5   
   �� 5      $��10 �6   �� 5      $��40 �6   �� 5      $��* �6   �� 5      $
��auto �6    � ColumnDefinitions 5   
   �� 5      $��10 �6   �� 5      $��* �6   �� 5      $��10 �6   .+�5   
   &   System.Windows.Controls.DataGrid  -   5          Name$ ProcessViewList��5      $��2��6E   $��1��6R   $��14(�6b      AutoGenerateColumns .     CanUserAddRows .     RowHeaderWidth$ 0Q�5   @   ��5   p   �� *ProcessList+   	CellStyle 5      �� 5      	 �� 
TargetType*  $System.Windows.Controls.DataGridCell"	  6   .�5      �� 
 �� Property
 �@J�IsHitTestVisible �� Value .  69   �� 5       
 � ��$ Center=�6D   �� 5!      
 � ��$ 2,2,2,2q�68   5"         Columns 5%      4  .System.Windows.Controls.DataGridTemplateColumn  5'      5(      0  *System.Windows.Controls.DataGridTextColumn  5,      ,  &System.Windows.Controls.DataGridColumn   Header$ Program Name��6%      FontSize$ 15Q�6;   1  +System.Windows.Controls.DataGridBoundColumn   Binding 5-   %   �� *Name+  5.      $ Process Name��6%   $ 15Q�6;      Width5  /System.Windows.Controls.DataGridLengthConverter$	 120 6I    5/   %   �� *PName+  50      $ Window Title��6%   $ 15Q�6;   $	 120 6I    51   %   �� *	WndName+  54      $ Total Hours��6%   $ 15Q�6:    55   %   �� *	ViewHours+ �� StringFormat$
 0.00��  56      $ 	Minimized��6%   $ 15Q�68    57   %   �� *
ViewMinH+$
 0.00��  58      $ Focused��6%   $ 15Q�66    59   %   �� *
ViewFocusH+$
 0.00��  5:      $ +Inputs:��6%   $ 15Q�67    5;   %   �� *
ViewInputH+$
 0.00��  5=      $ K��6%   $ 15Q�60    6>   �� *ViewInputKeyH+$
 0.00��   HeaderStyle 5>      �� 5?      =	  7System.Windows.Controls.Primitives.DataGridColumnHeader"	 	 6    .�5@      �� 
 � ��$ Center=�6K   5A      5C        5E      $ M��6%   $ 15Q�60    6>   �� *ViewInputMouseH+$
 0.00�� 5F      �� 5G      "	 	 6    .�5H      �� 
 � ��$ Center=�6K   5I      5K        5M      $	 K+M��6%   $ 15Q�62    6@   �� *ViewInputKMH+$
 0.00�� 5N      �� 5O      "	 	 6    .�5P      �� 
 � ��$ Center=�6K   5Q      5S        5U      $ C��6%   $ 15Q�60    6>   �� *ViewInputJoyH+$
 0.00�� 5V      �� 5W      "	 	 6    .�5X      �� 
 � ��$ Center=�6K   5Y      5[        5]      $ First Recorded Date��6%   $ 15Q�6B    5^   %   �� *First+  5_      $ Last Recorded Date��6%   $ 15Q�6A    5`   %   �� *Last+5b      �� 5d   
   $��1��6   $��1��6!   $��15(�61   $E��?Records and saves the running time of the elements on the list.��6?   �� 5e   
   $��3��6   $��1��6!   $��10(�61   $��Right=�6?   $#��ver 1.0.1  By DottierGalaxy50��6[   
�������6�   �� 5g   
   -   5         ��Name$ 	AddButton��5g   .   $��1��6   $��1��6   $	��100��5h      $��25��5i      $��Right=�5j      $��Add Program��5l      5r      5s      