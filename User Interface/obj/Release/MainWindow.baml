   M S B A M L   `   `   `  ���� I  EUser Interface, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null/clr-namespace:User_InterfaceUser_Interface      User_Interface.MainWindow   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     .localclr-namespace:User_Interface       � Title$  
MainWindow�� � WindowStartupLocation$ CenterScreen=� � WindowState$ 	Maximized=� � Icon$ MNM.JPG�� � 	Resources  �� Color   
MainColour  ButtonStyle �� 
TargetType �� Property �� Value	 �� Triggers
 P� Property P� Value  TextBoxStyle  ComboBoxStyle  ListBoxStyle  ProgressBarStyle  CanvasStyle  SliderStyle��%�  &	        &	      &	 �     &	 8    &	 �    &	 �    &	 E    &	 O    �� $ CornflowerBlue���� " ��.���  � ��$ 30����  � ��
 �   ���  � ��$ 12(���  � ��$ 5q���  �@�Cursor$
 Hand��	 P� 
 � �� . . ���  � ��
 ������� " ��.���  � ��$ 30����  � ��
 �   ���  � ��$ 12(���  � ��$ 5q��� " ��.���  � ��$ 30����  � ��
 �   ���  � ��$ 12(���  � ��$ 5q���  �@�Cursor$
 Hand���� " ��.���  � ��$ 70����  � ��$ 5q��� " �.���  � ��$ 10����  � ��
 � � ���  � ��$ 5q��� " ���� " ��.���  � ��$ 5q�.��� -    �Name$
 grid��$��5q�.+��� -   $��TubeMap��$
��1190��$
��1190�� � Cursor$ Arrow��$��Stretch=�
�������$
��Left=�.+��� -   $��CanvasLabel��$��Station�� �� Right$ 10�� �� Bottom$ 10��$	��232��$��Center=��� -   $��sideBarA��#���   $��
1200,0,0,0q�.+��� -   $��
LoadButton��$��	Load File��#��[ �� -   $��	ComboBoxA��#��[ �� -   $��	ComboBoxB��#��[ �� -   $��	CutButton��$��Cut Edge��#��[ �� -	   $��CutStationsListBox��#��[ $	��200��$��	5,5,4.8,5q��� -
   $��	RunButton��$	��Run��#��[ � -   $��StatusSoFar��#��[ �� -   $��SideBarB��#���   $y�Hidden=�$��
1200,0,0,0q�.+��� -   $��RestartButton��$��Restart��#��[ $
��Bold"� J� RenderTransformOrigin$ 	0.502,0.5&��� -   $��
SaveImages��$��Save Images��#��[ �� -   $��TimeBox��#��[ $��12:00�� �� 
IsReadOnly .  �� TextAlignment$ Center=��� -   $��
TimeSlider��#��[  � Maximum$	 143Q� �� TickFrequency$ 1Q� �� IsSnapToTickEnabled . 