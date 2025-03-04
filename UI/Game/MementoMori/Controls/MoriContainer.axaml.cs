﻿using Avalonia.ReactiveUI;
using NDBotUI.Modules.Game.MementoMori;
using NDBotUI.Modules.Game.MementoMori.Store;
using NDBotUI.Modules.Shared.EventManager;

namespace NDBotUI.UI.Game.MementoMori.Controls;

public partial class MoriContainer : ReactiveUserControl<MoriContainerViewModel>
{
    public MoriContainer()
    {
        InitializeComponent();
        MoriBoot.Boot();
        RxEventManager.Dispatch(MoriAction.InitMori.Create());
    }
}