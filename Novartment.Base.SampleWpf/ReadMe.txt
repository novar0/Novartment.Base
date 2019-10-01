1. Как начать пользоваться этим шаблоном
========================================
1. переименовать файл проекта и решения. внутри файла решения поправить ссылку на файл проекта
2. в свойствах проекта поправить Assembly name и Default Namespace
3. заменить namespace во всех исходниках (*.cs) и в xaml-файлах в значениях атрибута x:Class
4. В свойствах проекта на закладке Publish настроить параметры ClickOnce
5. добавить в проект Service Reference для используемого WCF-сервиса
6. вписать в AssemblyInfo.cs значения всех описательных атрибутов проекта (на работу программы они не влияют)
7. вписать в GlobalSetup.cs
	a) уникальный идентификатор приложения (использовать только символы, разрешаемые для имени файла) в свойство UniqueApplicationName
	b) ClickOnce URL приложения в свойство ClickOnceURL (null если не используется ClickOnce)
8. вписать в app.config
	a) URL WCF-сервиса в атрибут endpoint address 
	b) полное имя типа интерфейса WCF-сервиса (полученное на шаге 5) в атрибут endpoint contract
9. вписать в ComponentRegistration.cs
	a) в using ISerivceInterface вписать полное имя типа интерфейса WCF-сервиса (полученное на шаге 5)
	b) в using SerivceFaultExceptionData вписать полное имя типа объекта данных исключения WCF-сервиса (полученное на шаге 5)
10. AppSettings.cs
	a) Добавить свойства для хранения настроек проекта
11. Добавить необходимую функциональность в MainViewModel и MainWindow
12. Добавить необходимые модели и представления
13. По необходимости добавить провайдеры информации об исключениях:
	a) наследовать ExceptionDescriptionProviderBase
	b) переопределить те методы, которые могут получить дополнительную информацию об исключении
	c) в файле ComponentRegistration.cs добавить к списку зарегистрированных провайдеров


2. если WCF не используется
===========================
1. из пакетов убрать Autofac.Wcf, из References убрать System.ServiceModel.Activation и System.ServiceModel.Web
2. в файле ComponentRegistration.cs
	a) убрать using и builder.Register, связаные с веб-сервисом
	b) убрать класс ExceptionDescriptionProviderServiceFault и добавление его в список провайдеров информации об исключении
4. в файле app.config убрать раздел <system.serviceModel>
5. в файле MainViewModel.cs убрать аргумент веб-сервиса из конструктора и соответствующее ему поле (dataService)


2. использование динамической локализации интерфейса с помощью  ResxExtension
=============================================================================
RESX-файлы должны иметь Build Action = Embedded Resource

в декларации окна должна быть ссылка на библиотеку и указание умолчального RESX-файла чтобы использовать краткую форму локализации
<Window
	xmlns:bcl="clr-namespace:Novartment.Base.UI.Wpf;assembly=Novartment.BaseWindows"
	bcl:ResxExtension.DefaultResxName="Novartment.Base.SampleWpf.View.MainWindow"

2.1 локализация надписей
========================
краткая форма <TextBlock Text="{Resx TextBlockText1}"/>
или полная форма <TextBlock Text="{bcl:Resx ResxName=Novartment.Base.SampleWpf.View.MainWindow, Key=TextBlockText1}"/>

2.2 локализация нетекстовых свойств
===================================
<TextBlock Margin="{bcl:Resx Key=MyMargin, DefaultValue='18,0,0,71'}"/>

2.3 форматирование
==================
<bcl:Resx Key="MyFormatString" BindingElementName="_fileListBox" BindingPath="SelectedItem"/>
или
<bcl:Resx Key="MyMultiFormatString">
	<Resx BindingElementName="_fileListBox" BindingPath="Name"/>
	<Resx BindingElementName="_fileListBox" BindingPath="SelectedItem"/>
</bcl:Resx>

2.4 изменение языка во время исполнения
=======================================
Установите свойство Thread.CurrentThread.CurrentUICulture и потом вызывайте
Novartment.Base.UI.Wpf.ResxExtension.UpdateAllTargets ()

2.5 чтобы RESX-файлы в Solution Explorer выглядели вложенными в XAML
====================================================================
отдельную ссылку <EmbeddedResource Include="TestWindow.resx" />
заменить на вложенную
<EmbeddedResource Include="TestWindow.resx">
  <DependentUpon>TestWindow.xaml</DependentUpon>
  <SubType>Designer</SubType>
</EmbeddedResource>

либо можно использовать расширение "File Nesting"
http://visualstudiogallery.msdn.microsoft.com/3ebde8fb-26d8-4374-a0eb-1e4e2665070c