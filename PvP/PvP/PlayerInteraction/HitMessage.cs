using colonyserver.Assets.UIGeneration;
using ModLoaderInterfaces;
using Pipliz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PvP
{
    public class HitMessage : IOnUpdate
    {
        public static void ShowMessage(Players.Player toPlayer, String message)
        {
            string labelKey = "Hit" + toPlayer.Name;

            UIManager.AddorUpdateUILabel(labelKey, colonyshared.NetworkUI.UIGeneration.UIElementDisplayType.Global, message,
                                        new Vector3Int(40, 20, 0), colonyshared.NetworkUI.AnchorPresets.MiddleCenter,
                                        100, toPlayer, color: "#ff0000");

            showLabel.Enqueue((ServerTimeStamp.Now, (labelKey, toPlayer)));
        }

        public static Queue<(ServerTimeStamp, (String, Players.Player))> showLabel = new Queue<(ServerTimeStamp, (String, Players.Player))>();

        public void OnUpdate()
        {
            if (showLabel.Count == 0)
                return;

            var label = showLabel.Peek();

            if (label.Item1.TimeSinceThis < 500L)
                return;

            UIManager.RemoveUILabel(label.Item2.Item1, label.Item2.Item2);
            showLabel.Dequeue();
        }
    }
}
