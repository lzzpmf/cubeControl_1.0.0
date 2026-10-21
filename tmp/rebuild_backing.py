from pathlib import Path
import re
p=Path('CubeControl-1.1/Cube/windows/Cube.Desktop/MainWindow.xaml')
s=p.read_text(encoding='utf-8-sig')
start=s.index('   <TabItem Header="{loc:Translate Text=\'Backing tracks\'}">')
end=s.index('   <TabItem Header="{loc:Translate Text=\'Criador de amps\'}">',start)
old=s[start:end]
Path('tmp/backing-before-studio.xaml').write_text(old,encoding='utf-8')
yt=old[old.index('<TabItem Header="YouTube">'):old.index('</TabControl></DockPanel></Border><ContentControl')]
# This slice contains the complete existing YouTube tab and its connected handlers.
def tr(t):return "{loc:Translate Text='"+t+"'}"
def toggles():
 return '<StackPanel Orientation="Horizontal">'+''.join(f'<CheckBox Content="{letter}" IsChecked="{{Binding {prop}}}" Style="{{StaticResource StudioToggle}}" AutomationProperties.Name="{tr(label)}" ToolTip="{tr(label)}"'+(' IsEnabled="{Binding CanEditRouting}"' if letter in 'RI' else '')+'/>' for letter,prop,label in [('S','IsSolo','Solo'),('M','IsMuted','Mute'),('R','IsArmed','Armar gravação'),('I','IsMonitoring','Monitorar entrada')])+'</StackPanel>'
def inputbox():return '<ComboBox ItemsSource="{Binding DataContext.InputChoices,RelativeSource={RelativeSource AncestorType=Window}}" SelectedItem="{Binding InputChoice}" DisplayMemberPath="Label" IsEnabled="{Binding CanEditRouting}" MinHeight="24" Height="24" Padding="3,0" AutomationProperties.Name="'+tr('Canais de entrada')+'"/>'
def pan():return '<StackPanel HorizontalAlignment="Center"><local:Knob Width="38" Height="38" Minimum="-1" Maximum="1" StepSize="0.01" Value="{Binding Pan}" AutomationProperties.Name="Pan"/><TextBlock Text="L   Pan   R" FontSize="10" HorizontalAlignment="Center"/></StackPanel>'
def button(label,attr):return '<ui:Button Content="'+tr(label)+'" '+attr+'/>'
page='''   <TabItem Header="{loc:Translate Text='Backing tracks'}">
    <Grid x:Name="BackingPage" local:SurfaceMotion.Enabled="True" local:SurfaceMotion.Offset="8" DataContext="{Binding BackingTracks,RelativeSource={RelativeSource AncestorType=Window}}" SizeChanged="BackingPageSizeChanged" AllowDrop="True">
     <Grid.Resources>
      <Style x:Key="StudioToggle" TargetType="CheckBox" BasedOn="{StaticResource SessionMute}"><Setter Property="Width" Value="25"/><Setter Property="Height" Value="25"/><Setter Property="Margin" Value="0,0,3,0"/><Setter Property="Foreground" Value="{DynamicResource CubeText}"/><Style.Triggers><Trigger Property="IsChecked" Value="True"><Setter Property="Foreground" Value="{DynamicResource CubeAccent}"/></Trigger></Style.Triggers></Style>
      <Style TargetType="ui:Button" BasedOn="{StaticResource CubeButtonStyle}"><Setter Property="MinHeight" Value="40"/><Setter Property="Margin" Value="0,0,8,0"/></Style>
      <Style x:Key="StudioDivider" TargetType="Border"><Setter Property="Width" Value="1"/><Setter Property="Margin" Value="12,4"/><Setter Property="Background" Value="{DynamicResource CubeBorder}"/></Style>
      <Style x:Key="StudioCaption" TargetType="TextBlock" BasedOn="{StaticResource CaptionText}"><Setter Property="Foreground" Value="{DynamicResource Muted}"/></Style>
     </Grid.Resources>
     <Grid.RowDefinitions><RowDefinition Height="68"/><RowDefinition Height="*"/></Grid.RowDefinitions>
     <Grid.ColumnDefinitions><ColumnDefinition Width="*"/><ColumnDefinition x:Name="BackingSideColumn" Width="310"/></Grid.ColumnDefinitions>
     <Border x:Name="BackingTransportHost" Grid.ColumnSpan="2" BorderBrush="{DynamicResource CubeBorder}" BorderThickness="0,0,0,1" Background="{DynamicResource CubeInset}">
      <ScrollViewer HorizontalScrollBarVisibility="Auto" VerticalScrollBarVisibility="Disabled"><StackPanel x:Name="BackingTransportButtons" Orientation="Horizontal" VerticalAlignment="Center" Margin="12,4">
       <StackPanel Width="180" Margin="0,0,20,0"><TextBlock Text="{loc:Translate Text='Dispositivo de áudio'}" Style="{StaticResource StudioCaption}"/><ComboBox ItemsSource="{Binding Outputs}" SelectedItem="{Binding SelectedOutput}" DisplayMemberPath="Name" IsEnabled="{Binding CanEditSession}" Height="28" MinHeight="28" Padding="4,0" AutomationProperties.Name="{loc:Translate Text='Saída do acompanhamento'}"/></StackPanel>
       <ui:Button Command="{Binding ToggleRecording}" ToolTip="{loc:Translate Value={Binding RecordingHelp}}" AutomationProperties.Name="{loc:Translate Value={Binding RecordingLabel}}"><StackPanel Orientation="Horizontal"><Ellipse Width="16" Height="16" Fill="{DynamicResource SystemFillColorCriticalBrush}" Margin="0,0,8,0"/><TextBlock Text="Rec"/></StackPanel></ui:Button>
       <ui:Button x:Name="BackingPlayButton" Command="{Binding PlayPause}" Width="42" AutomationProperties.Name="{loc:Translate Value={Binding PlayLabel}}"><ui:Button.Style><Style TargetType="ui:Button" BasedOn="{StaticResource CubeButtonStyle}"><Setter Property="Content" Value="▶"/><Style.Triggers><DataTrigger Binding="{Binding IsPlaying}" Value="True"><Setter Property="Content" Value="Ⅱ"/></DataTrigger></Style.Triggers></Style></ui:Button.Style></ui:Button>
       <ui:Button Command="{Binding Stop}" Content="■" Width="42" AutomationProperties.Name="{loc:Translate Text='Parar'}"/>
       <CheckBox x:Name="BackingLoopButton" Content="{loc:Translate Text='Loop'}" IsChecked="{Binding IsLooping}" IsEnabled="{Binding CanLoop}" VerticalAlignment="Center"/>
       <Border Style="{StaticResource StudioDivider}"/>
       <StackPanel x:Name="BackingMetronomePanel" Orientation="Horizontal" VerticalAlignment="Center"><Control Style="{StaticResource CubeMetronomeIcon}" Width="24" Height="24" Margin="0,0,8,0"/><CheckBox Content="{loc:Translate Text='Metrônomo'}" IsChecked="{Binding MetronomeEnabled}"/></StackPanel>
       <Border Style="{StaticResource StudioDivider}"/><TextBlock Text="BPM" VerticalAlignment="Center" Margin="0,0,8,0"/>
       <Slider Width="120" Minimum="20" Maximum="300" TickFrequency="1" IsSnapToTickEnabled="True" Value="{Binding TransportBpm}" IsEnabled="{Binding CanEditSession}" VerticalAlignment="Center" AutomationProperties.Name="BPM"/>
       <ui:Button x:Name="BackingBpmButton" Content="{Binding TransportBpm,StringFormat={}{0:0} BPM}" Width="80" Click="BackingBpmClick"/>
       <Border Style="{StaticResource StudioDivider}"/>
       <StackPanel Width="102"><TextBlock Text="{loc:Translate Text='Velocidade'}" Style="{StaticResource StudioCaption}"/><ComboBox x:Name="BackingSpeedOptions" ItemsSource="{Binding StudioSpeeds}" SelectedItem="{Binding Speed}" Height="28" MinHeight="28" Padding="4,0" IsEnabled="{Binding CanEditSession}" AutomationProperties.Name="{loc:Translate Text='Velocidade'}"><ComboBox.ItemTemplate><DataTemplate><TextBlock Text="{Binding StringFormat={}{0:0}%}"/></DataTemplate></ComboBox.ItemTemplate></ComboBox></StackPanel>
       <Border Style="{StaticResource StudioDivider}"/><StackPanel Width="104"><TextBlock Text="{loc:Translate Text='Tom'}" Style="{StaticResource StudioCaption}"/><ComboBox x:Name="BackingPitchOptions" ItemsSource="{Binding StudioPitches}" SelectedItem="{Binding Pitch}" Height="28" MinHeight="28" Padding="4,0" IsEnabled="{Binding CanEditSession}" AutomationProperties.Name="{loc:Translate Text='Tom'}"><ComboBox.ItemTemplate><DataTemplate><TextBlock Text="{Binding StringFormat={}{0:+0;-0;0} st}"/></DataTemplate></ComboBox.ItemTemplate></ComboBox></StackPanel>
       <Border Style="{StaticResource StudioDivider}"/>
       <ui:Button Content="{loc:Translate Text='Cortar'}" Command="{Binding CutAtCursor}"/>
       <ui:Button Content="{loc:Translate Text='Importar'}" Command="{Binding ImportStem}"/>
       <ui:Button Content="{loc:Translate Text='Exportar'}" Command="{Binding ExportRange}"/>
       <ui:Button Content="⋯" Click="StudioOptionsClick" AutomationProperties.Name="{loc:Translate Text='Opções'}"/>
      </StackPanel></ScrollViewer>
     </Border>
     <Grid Grid.Row="1" x:Name="BackingWorkspace"><Grid.RowDefinitions><RowDefinition Height="59*"/><RowDefinition Height="41*"/></Grid.RowDefinitions>
      <Grid x:Name="BackingArrangement" Background="{DynamicResource CubeWell}"><Grid.RowDefinitions><RowDefinition Height="52"/><RowDefinition Height="*"/></Grid.RowDefinitions>
       <Grid><TextBlock Text="{loc:Translate Text='Compassos'}" Margin="12,8" Style="{StaticResource StudioCaption}"/><local:BackingWaveform x:Name="BackingMainWave" Margin="335,0,0,0" Duration="{Binding StudioDuration}" Position="{Binding Position,Mode=TwoWay}" RangeStart="{Binding RangeStart,Mode=TwoWay}" RangeEnd="{Binding RangeEnd,Mode=TwoWay}" IsLooping="{Binding IsLooping}" Bpm="{Binding TransportBpm}" Meter="{Binding SessionMeter}" FirstBeat="{Binding FirstBeatOffset}" ViewportStart="{Binding ViewportStart}" ViewportDuration="{Binding StudioViewportDuration}" MusicalRuler="True"/></Grid>
       <ListBox x:Name="BackingStemList" Grid.Row="1" ItemsSource="{Binding SessionStems}" SelectedItem="{Binding SelectedStem}" AlternationCount="100000" BorderThickness="0" Background="Transparent" HorizontalContentAlignment="Stretch" ScrollViewer.HorizontalScrollBarVisibility="Disabled" ScrollViewer.VerticalScrollBarVisibility="Auto" LayoutUpdated="SessionTrackLayoutUpdated" SizeChanged="SessionTrackWidthChanged" AllowDrop="True" PreviewMouseWheel="BackingTrackHeightWheel" AutomationProperties.Name="{loc:Translate Text='Pistas da sessão'}">
        <ListBox.ItemContainerStyle><Style TargetType="ListBoxItem" BasedOn="{StaticResource CompactMusicListItem}"><Setter Property="Padding" Value="0"/><Setter Property="Margin" Value="0"/><Setter Property="HorizontalContentAlignment" Value="Stretch"/></Style></ListBox.ItemContainerStyle>
        <ListBox.ItemTemplate><DataTemplate><Grid Height="96"><Grid.ColumnDefinitions><ColumnDefinition Width="335"/><ColumnDefinition Width="*"/></Grid.ColumnDefinitions>
         <Border Background="{DynamicResource CubeInset}" BorderBrush="{DynamicResource CubeBorder}" BorderThickness="0,0,1,1" ContextMenuOpening="SessionStemContextOpening"><Border.ContextMenu><ContextMenu/></Border.ContextMenu>
          <Grid Margin="0,0,8,0"><Grid.ColumnDefinitions><ColumnDefinition Width="10"/><ColumnDefinition Width="136"/><ColumnDefinition Width="108"/><ColumnDefinition Width="65"/></Grid.ColumnDefinitions>
           <Border Background="{Binding Color}" Margin="0,2,0,2" CornerRadius="2"/>
           <StackPanel Grid.Column="1" Margin="8,3,4,3"><DockPanel><TextBlock Text="{Binding RelativeSource={RelativeSource AncestorType=ListBoxItem},Path=(ItemsControl.AlternationIndex),Converter={StaticResource StudioIndex}}" Margin="0,0,6,0"/><TextBox Text="{Binding Name,UpdateSourceTrigger=LostFocus}" MinHeight="0" Height="22" Padding="0" BorderThickness="0" AutomationProperties.Name="{loc:Translate Text='Nome da pista'}"/></DockPanel>TOGGLES<Border Height="5"/>INPUT</StackPanel>
           <StackPanel Grid.Column="2" VerticalAlignment="Center"><TextBlock Text="{Binding GainDb,StringFormat={}{0:0.0} dB}" Style="{StaticResource StudioCaption}"/><Slider Minimum="-61" Maximum="6" Value="{Binding GainDb}" Width="96" AutomationProperties.Name="{loc:Translate Text='Volume da pista'}"/></StackPanel>
           <ContentControl Grid.Column="3" VerticalAlignment="Center">PAN</ContentControl>
          </Grid>
         </Border>
         <Grid Grid.Column="1"><local:BackingWaveform Visibility="Hidden" IsHitTestVisible="False" ShowRuler="False" Duration="{Binding DataContext.StudioDuration,RelativeSource={RelativeSource AncestorType=ListBox}}"/><local:BackingClipLane Model="{Binding DataContext,RelativeSource={RelativeSource AncestorType=ListBox}}" Stem="{Binding}"/></Grid>
        </Grid></DataTemplate></ListBox.ItemTemplate>
       </ListBox>
      </Grid>
      <Border x:Name="BackingVolumesPanel" Grid.Row="1" BorderBrush="{DynamicResource CubeBorder}" BorderThickness="0,1,0,0" Padding="0,8,0,0">
       <ContentControl x:Name="SessionMixerHost" HorizontalContentAlignment="Stretch" VerticalContentAlignment="Stretch"><DockPanel x:Name="SessionMixerContent">
        <Border DockPanel.Dock="Right" Width="110" Margin="4,0,0,0" Background="{DynamicResource CubeInset}" BorderBrush="{DynamicResource CubeAccent}" BorderThickness="1" CornerRadius="{DynamicResource CubeCardCornerRadius}"><ScrollViewer VerticalScrollBarVisibility="Auto" HorizontalScrollBarVisibility="Disabled"><StackPanel Margin="6"><TextBlock Text="Master" HorizontalAlignment="Center" Height="30"/><Border Height="126"/><StackPanel Orientation="Horizontal"><local:BackingFaderMeter Reading="{Binding MasterMeter}" Vertical="True" Width="28" Height="160"/><local:BackingGainControl Vertical="True" GainDb="{Binding MasterDb}" AutomationProperties.Name="Master"/></StackPanel></StackPanel></ScrollViewer></Border>
        <ScrollViewer HorizontalScrollBarVisibility="Auto" VerticalScrollBarVisibility="Auto"><ItemsControl ItemsSource="{Binding SessionStems}" AlternationCount="100000"><ItemsControl.ItemsPanel><ItemsPanelTemplate><StackPanel Orientation="Horizontal"/></ItemsPanelTemplate></ItemsControl.ItemsPanel><ItemsControl.ItemTemplate><DataTemplate>
         <Border Width="110" Margin="0,0,4,0" Background="{DynamicResource CubeInset}" BorderBrush="{DynamicResource CubeBorder}" BorderThickness="1" CornerRadius="{DynamicResource CubeCardCornerRadius}"><StackPanel Margin="5">
          <DockPanel Height="28"><Border Width="5" Background="{Binding Color}" Margin="-5,-5,5,0"/><TextBlock Text="{Binding Title}" TextTrimming="CharacterEllipsis" ToolTip="{Binding Title}"/></DockPanel>
          INPUT<Border Height="6"/>TOGGLES<Border Height="6"/>PAN<Border Height="8"/>
          <StackPanel Orientation="Horizontal"><local:BackingFaderMeter Reading="{Binding Meter}" Vertical="True" Width="28" Height="160"/><local:BackingGainControl Vertical="True" GainDb="{Binding GainDb}" AutomationProperties.Name="{loc:Translate Text='Volume da pista'}"/></StackPanel>
         </StackPanel></Border>
        </DataTemplate></ItemsControl.ItemTemplate></ItemsControl></ScrollViewer>
       </DockPanel></ContentControl>
      </Border>
     </Grid>
     <Border x:Name="BackingLibraryLayout" Grid.Row="1" Grid.Column="1" Panel.ZIndex="10" Background="{DynamicResource CubeFloatingSurface}" BorderBrush="{DynamicResource CubeBorder}" BorderThickness="1,0,0,0" Padding="12,8"><DockPanel>
      <DockPanel DockPanel.Dock="Top" Margin="0,0,0,8"><ui:Button DockPanel.Dock="Right" Content="»" Click="ToggleBackingLibrary" Width="32" MinWidth="0" AutomationProperties.Name="{loc:Translate Text='Fechar biblioteca'}"/><TextBlock Text="{loc:Translate Text='Biblioteca de áudio'}" Style="{StaticResource BodyStrongText}" VerticalAlignment="Center"/></DockPanel>
      <TabControl><TabItem Header="{loc:Translate Text='Arquivos locais'}"><DockPanel>
       <StackPanel DockPanel.Dock="Top" Margin="0,8"><TextBox Text="{Binding LocalSearch,UpdateSourceTrigger=PropertyChanged}" AutomationProperties.Name="{loc:Translate Text='Buscar arquivos'}"/>
        <ListBox ItemsSource="{Binding LocalCategories}" SelectedItem="{Binding LocalCategory}" BorderThickness="0" Margin="0,8"><ListBox.ItemsPanel><ItemsPanelTemplate><StackPanel Orientation="Horizontal"/></ItemsPanelTemplate></ListBox.ItemsPanel><ListBox.ItemTemplate><DataTemplate><TextBlock Text="{loc:Translate Value={Binding}}" FontSize="12"/></DataTemplate></ListBox.ItemTemplate></ListBox>
        <ui:Button Content="{loc:Translate Text='Adicionar pasta'}" Command="{Binding AddLibraryFolder}"/>
       </StackPanel>
       <ListBox ItemsSource="{Binding LocalFiles}" BorderThickness="0" ScrollViewer.HorizontalScrollBarVisibility="Disabled" HorizontalContentAlignment="Stretch" AutomationProperties.Name="{loc:Translate Text='Biblioteca de áudio'}"><ListBox.ItemTemplate><DataTemplate>
        <Border BorderBrush="{DynamicResource CubeBorder}" BorderThickness="1" CornerRadius="{DynamicResource CubeCardCornerRadius}" Padding="4" Margin="0,0,0,4" Background="{DynamicResource CubeInset}" ContextMenuOpening="LocalFileContextOpening" PreviewMouseMove="LocalFileDrag">
         <Grid><Grid.ColumnDefinitions><ColumnDefinition Width="42"/><ColumnDefinition Width="*"/><ColumnDefinition Width="32"/></Grid.ColumnDefinitions><ui:Button Content="▶" Tag="{Binding}" Click="PreviewLocalFile" MinWidth="0" Width="38" Height="42" Margin="0" AutomationProperties.Name="{loc:Translate Text='Pré-ouvir'}"/><StackPanel Grid.Column="1" Margin="8,0" VerticalAlignment="Center"><TextBlock Text="{Binding Name}" TextTrimming="CharacterEllipsis" ToolTip="{Binding Path}"/><TextBlock Text="{Binding Details}" Style="{StaticResource StudioCaption}"/></StackPanel><ui:Button Grid.Column="2" Content="↓" Tag="{Binding}" Click="InsertLocalFile" Width="30" MinWidth="0" Margin="0" AutomationProperties.Name="{loc:Translate Text='Importar áudio'}"/></Grid>
        </Border>
       </DataTemplate></ListBox.ItemTemplate></ListBox>
      </DockPanel></TabItem>YOUTUBE</TabControl>
     </DockPanel></Border>
     <Border x:Name="BackingDropOverlay" Grid.Row="1" Grid.ColumnSpan="2" Visibility="Collapsed" IsHitTestVisible="False"/>
     <ui:InfoBar Grid.Row="1" Grid.ColumnSpan="2" VerticalAlignment="Bottom" IsOpen="{Binding HasStatus}" Message="{loc:Translate Value={Binding Status}}" Severity="{Binding StatusSeverity}"/>
     <Popup x:Name="BackingSessionsPopup" PlacementTarget="{Binding ElementName=BackingTransportHost}" Placement="Bottom" StaysOpen="False"><Border Background="{DynamicResource CubeFloatingSurface}" Padding="12"><StackPanel Width="300"><TextBlock Text="{loc:Translate Text='Sessões'}"/><ListBox x:Name="BackingTrackList" ItemsSource="{Binding Tracks}" SelectedItem="{Binding SelectedTrack}" DisplayMemberPath="Title" MaxHeight="300"/><ui:Button Content="{loc:Translate Text='Nova pista'}" Command="{Binding AddEmptyStem}"/><ComboBox ItemsSource="{Binding Inputs}" SelectedItem="{Binding SelectedInput}" DisplayMemberPath="Name" IsEnabled="{Binding CanChangeRecordingInput}" AutomationProperties.Name="{loc:Translate Text='Entrada de áudio'}"/></StackPanel></Border></Popup>
    </Grid>
   </TabItem>
'''
page=page.replace('TOGGLES',toggles()).replace('INPUT',inputbox()).replace('PAN',pan()).replace('YOUTUBE',yt)
page=page.replace('<Grid.Resources>','<Grid.Resources><local:StudioIndexConverter x:Key="StudioIndex"/>',1)
s=s[:start]+page+s[end:]
p.write_text(s,encoding='utf-8')
