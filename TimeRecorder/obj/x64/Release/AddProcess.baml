   M S B A M L   `   `   `  ���� G  CTimeRecorder, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null+clr-namespace:TimeRecorderTimeRecorder      TimeRecorder.AddProcess   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     ,localclr-namespace:TimeRecorder       � Title$  Add Program��$	��410��$	��540��.���  � RowDefinitions �� $��20 ��� $��* ��� $��20 � � ColumnDefinitions �� $��20 ��� $��* ��� $��20 �.+��� $��1��$��1��$��14(�$
��Left=�$	��Top=�$��Name:���� -    ��Name$ NameTextBox��$��1��$��1�� �� TextWrapping$
 Wrap=�$
��Left=�$	��Top=�$	��125�� �� 	MaxLength$ 50�� �� MaxLines$ 1��$��50,5,0,0q��� $��1��$��1��$��14(�$
��Left=�$	��Top=�$��Program's Path (.exe):��$��0,34,0,0q��� -   $ 
DirTextBox��$��1��$��1��$
 Wrap=�$
��Left=�$	��Top=�$	��290��$
 1000��$ 1��$��
143,39,0,0q��� -    ��Name$ AddFind��$��1��$��1��$
��Left=�$	��Top=�$��Search��$��
438,38,0,0q��� $��1��$��1��$��14(�$
��Left=�$	��Top=�$��Total Hours:��$��0,68,0,0q��� -   $ HoursTextBox��$��1��$��1��$
 Wrap=�$
��0.00��$
��Left=�$	��Top=�$��65��$ 9��$��	83,73,0,0q��� -    ��Name$ UseWndCheckBox��$��1��$��1��$��14(�$
��Left=�$	��Top=�$��Record a Window by Title:��$��	5,113,0,0q��� -   $ MatchModeCheckBox��$��1��$��1��$
��Left=�$	��Top=�$��Match Exactly��	 d� 	IsChecked$	 falseY�$��368,114,0,0q��� -   $ WndNameTextBox��$��1��$��1��$
 Wrap=�$
��Left=�$	��Top=�$	��100��$ 50��$ 1��$��191,113,0,0q�$	��170���� $��1��$��1��$��14(�$
��Left=�$	��Top=�$ ��Use specific icon on list:��$��	0,144,0,0q��� -   $ IcoDirTextBox��$��1��$��1��$
 Wrap=�$
��Left=�$	��Top=�$	��277��$
 1000��$ 1��$��156,150,0,0q��� -	   $ 
IcoAddFind��$��1��$��1��$
��Left=�$	��Top=�$��Search��$��438,149,0,0q��� -
   $ UseWndIconCheckBox��$��1��$��1��$��14(�$
��Left=�$	��Top=�$?��9Use the first recorded window's icon on the list instead.��$��	5,179,0,0q��� $��1��$��14(�$
��Left=�$	��Top=�$3��-Seconds to keep recording for after an input:��$��	0,211,0,0q�$��1���� -   $ InputWaitTextBox��$��1��$��1��$
 Wrap=�$
��1.00��$
��Left=�$	��Top=�$��65��$ 9��$��287,216,0,0q��� $��1��$
��Left=�$	��Top=�$��	0,245,0,0q�$��1��.���� $��14(�$k��eAmount of seconds to save if there were no new inputs while recording for
the specified amount above:���� -   $ InputSaveTextBox��$��1��$��1��$
 Wrap=�$
��0.00��$
��Left=�$	��Top=�$��65��$ 9��$��185,271,0,0q��� -   $ 	AddButton��$��1��$��1��$	��100��$��25��$��Right=�$��Bottom=�$��Add Program��