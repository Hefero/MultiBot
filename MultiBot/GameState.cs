using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Enigma.D3.ApplicationModel;
using Enigma.D3.Assets;
using Enigma.D3.AttributeModel;
using Enigma.D3.DataTypes;
using Enigma.D3.Enums;
using Enigma.D3.MemoryModel;
using Enigma.D3.MemoryModel.Core;
using Enigma.D3;
using Enigma.D3.MemoryModel.Controls;
using static Enigma.D3.MemoryModel.Core.UXHelper;
using System.Diagnostics;

namespace EnvControllers
{
    public class GameState
    {
        //Memory context and Snapshot
        public MemoryContext ctx { get; }
        public ApplicationSnapshot snapshot {
            get
            {
                var _snapshot = new ApplicationSnapshot(this.ctx);
                try
                {
                    _snapshot.Update();
                    return _snapshot;
                }
                catch
                {
                    return _snapshot;
                }
                
            }
            set { }
        }
        public GameState() {
            ctx = MemoryContext.FromProcess();
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            lastRift = stopwatch;
            UpdateGameState();
            clientWidth = snapshot.Window.ClientRect.Width;
            clientHeight = snapshot.Window.ClientRect.Height;
        }

        public void UpdateSnapshot() {
            var _snapshot = new ApplicationSnapshot(this.ctx);
            try
            {
                _snapshot.Update();
                this.snapshot = _snapshot;
            }
            catch
            {

            }            
        }

        public float clientWidth { get; set; }
        public float clientHeight { get; set; }

        public void UpdateGameState()
        {
            UpdateSnapshot();
            var _intoGRift = intoGRift;
        }

        //Properties UX
        public UXControl openriftUiControl { get; set; }
        public UXControl acceptgrUiControl { get; set; }
        public UXControl urshiUiControl { get; set; }
        public UXControl grcompleteUiControl { get; set; }
        public UXControl confirmationUiControl { get; set; }
        public UXControl vendorUiControl { get; set; }
        public UXControl leavegameUiControl { get; set; }
        public UXControl player1UiControl { get; set; }
        public UXControl player1UiBusyControl { get; set; }

        //State Properties
        public bool inGame {

            get {
                return ctx.DataSegment.LocalData.IsPlayerValid;
            }
            set { }
        }

        public bool inMenu
        {
            get
            {
                return ctx.DataSegment.LocalData.IsStartUpGame;
            }
            set { }
        }

        public bool isLoading
        {
            get
            {
                if (ctx.DataSegment.LocalData.IsPlayerValid & ctx.DataSegment.LocalData.IsStartUpGame)
                {
                    return true;
                }
                if (!ctx.DataSegment.LocalData.IsPlayerValid & !ctx.DataSegment.LocalData.IsStartUpGame)
                {
                    return true;
                }
                else
                {
                    return false;
                }
                
            }

            set { }
        }

        public Stopwatch lastRift { get; set; }


        public bool openriftUiVisible {
            get
            {
                if (inMenu == false)
                {
                    try
                    {
                        openriftUiControl = UXHelper.GetControl<UXControl>("Root.NormalLayer.rift_dialog_mainPage.LayoutRoot.MainBG");
                        return openriftUiControl.IsVisible();
                    }
                    catch { return false; }
                }
                else{
                    return false;
                }
            }
        }
        
        public bool acceptgrUiVisible
        {
            get
            {
                if (inMenu == false)
                {
                    try
                    {
                        acceptgrUiControl = UXHelper.GetControl<UXControl>("Root.NormalLayer.rift_join_party_main.stack.wrapper.Accept");
                        return acceptgrUiControl.IsVisible();
                    }
                    catch { return false; }
                }
                else
                {
                    return false;
                }
            }
        }
        
        public bool urshiUiVisible
        {
            get
            {
                if (inMenu == false)
                {
                    try
                    {
                        urshiUiControl = UXHelper.GetControl<UXControl>("Root.NormalLayer.vendor_dialog_mainPage.riftReward_dialog.LayoutRoot.gemUpgradePane.centerWisp");
                        return urshiUiControl.IsVisible();
                    }
                    catch { return false; }
                }
                else
                {
                    return false;
                }
            }
        }
        
        public bool grcompleteUiVisible
        {
            get
            {
                if (inMenu == false)
                {
                    try
                    {
                        grcompleteUiControl = UXHelper.GetControl<UXControl>("Root.NormalLayer.GreaterRifts_VictoryScreen.LayoutRoot.PartyContainer.PlayerContainer.Player1.Player1Profile.Player1Banner.Player1Clan");
                        return grcompleteUiControl.IsVisible();
                    }
                    catch { return false; }
                }
                else
                {
                    return false;
                }
            }
        }
        
        public bool confirmationUiVisible
        {
            get
            {
                if (inMenu == false)
                {
                    try
                    {
                        confirmationUiControl = UXHelper.GetControl<UXControl>("Root.TopLayer.confirmation.subdlg.stack.wrap.button_cancel");
                        return confirmationUiControl.IsVisible();
                    }
                    catch { return false; }
                }
                else
                {
                    return false;
                }
            }
        }
        
        public bool vendorUiVisible
        {
            get
            {
                if (inMenu == false)
                {
                    try
                    {
                        vendorUiControl = UXHelper.GetControl<UXControl>("Root.NormalLayer.vendor_dialog_mainPage");
                        return vendorUiControl.IsVisible();
                    }
                    catch { return false; }
                }
                else
                {
                    return false;
                }
            }
        }
        
        public bool leavegameUiVisible
        {
            get
            {
                if (inMenu == false)
                {
                    try
                    {
                        leavegameUiControl = UXHelper.GetControl<UXControl>("Root.NormalLayer.gamemenu_dialog.gamemenu_bkgrnd.ButtonStackContainer.button_leaveGame");
                        return leavegameUiControl.IsVisible();
                    }
                    catch { return false; }
                }
                else
                {
                    return false;
                }
            }
        }

        
        public bool player1UiVisible
        {
            get
            {
                if (inMenu == false)
                {
                    try
                    {
                        player1UiControl = UXHelper.GetControl<UXControl>("Root.NormalLayer.portraits.stack.party_stack.portrait_1.Frame");
                        return player1UiControl.IsVisible();
                    }
                    catch { return false; }
                }
                else
                {
                    return false;
                }
            }
        }

        
        public bool player1UiBusyVisible
        {
            get
            {
                if (inMenu == false)
                {
                    try
                    {
                        player1UiBusyControl = UXHelper.GetControl<UXControl>("Root.NormalLayer.portraits.stack.party_stack.portrait_1.busy");
                        return player1UiBusyControl.IsVisible();
                    }
                    catch { return false; }
                }
                else
                {
                    return false;
                }
            }
        }

        //Other Properties
        public bool cancelgriftUiVisible
        {
            get
            {
                if (confirmationUiVisible == true & vendorUiVisible == false)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

        }
        public bool firstlevelRift
        {
            get
            {
                if (ctx.DataSegment.LevelArea.LevelAreaSNO.Value == 288482)
                {
                    lastRift.Restart();
                    return true;
                }
                else
                {
                    return false;
                }

            }
        }
        public bool intoGRift
        {
            get
            {
                uint CurrentArea = ctx.DataSegment.LevelArea.LevelAreaSNO.Value;
                switch (CurrentArea)
                {
                    case 288482:
                    case 288684:
                    case 288686:
                    case 288797:
                    case 288799:
                    case 288801:
                    case 288803:
                    case 288809:
                    case 288812:
                    case 288813:
                        {
                            lastRift.Restart();
                            return true;
                        }
                    default:
                        {
                            return false;
                        }
                }
            }
        }
        public bool haveUrshiActor
        {
            get
            {
                try
                {
                    if (snapshot.Game.Monsters.FirstOrDefault(ab => ab.ActorSNO == 398682) != null) //Urshi ID
                    {
                        lastRift.Restart();
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch { return false; }
            }
        }
        public bool aloneInGame
        {
            get
            {
                try
                {
                    if (inGame & !inMenu & !isLoading & !player1UiBusyVisible & !player1UiVisible & ctx.DataSegment.LocalData.PlayerCount == 1)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch { return false; }
            }
        }
    }
}

