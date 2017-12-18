using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;

namespace starPursuitClient
{
    public class starPursuitClient: BaseScript
    {
        List<Tracker> trackers = new List<Tracker>();
        Tracker currentTracker;
        List<Blip> blipPool = new List<Blip>();
        bool isTracking = false;
        bool timeOut = false;

        public starPursuitClient()
        {
            Tick += OnTick;
            RegisterCommand("etracker", new Action<int, List<dynamic>, string>((source, args, rawCommand) => { ETrackerC(source, args, rawCommand); }), false);
            RegisterCommand("dtracker", new Action<int, List<dynamic>, string>((source, args, rawCommand) => { DTrackerC(source, args, rawCommand); }), false);
            RegisterCommand("ctracker", new Action<int, List<dynamic>, string>((source, args, rawCommand) => { CTrackerC(source, args, rawCommand); }), false);
            RegisterCommand("ctrackers", new Action<int, List<dynamic>, string>((source, args, rawCommand) => { CTrackersC(source, args, rawCommand); }), false);
            RegisterCommand("ltrackers", new Action<int, List<dynamic>, string>((source, args, rawCommand) => { LTrackersC(source, args, rawCommand); }), false);
        }

        private void ETrackerC(int sourceID, List<dynamic> args, string rawCommand)
        {
            if(args.Count == 1)
            {
                ETracker(Convert.ToInt32(args[0]));
            }
            else
            {
                TriggerEvent("chatMessage", "Star Pursuit", new[] { 255, 0, 0 }, "Invalid Syntax, use: /etracker <Tracker ID>");
            }
        }

        private void DTrackerC(int sourceID, List<dynamic> args, string rawCommand)
        {
            DTracker();
        }

        private void CTrackerC(int sourceID, List<dynamic> args, string rawCommand)
        {
            if (args.Count == 1)
            {
                CTracker(Convert.ToInt32(args[0]));
            }
            else
            {
                TriggerEvent("chatMessage", "Star Pursuit", new[] { 255, 0, 0 }, "Invalid Syntax, use: /etracker <Tracker ID>");
            }
        }

        private void CTrackersC(int sourceID, List<dynamic> args, string rawCommand)
        {
            CTrackers();
        }

        private void LTrackersC(int sourceID, List<dynamic> args, string rawCommand)
        {
            LTrackers();
        }

        private void CTracker(int trackerID)
        {
            if (GetTrackerFromID(trackerID) != null)
            {
                TriggerEvent("chatMessage", "Star Pursuit", new[] { 255, 0, 0 }, "Deleted Tracker #" + GetTrackerFromID(trackerID).TrackerID);
                if (isTracking && currentTracker == GetTrackerFromID(trackerID))
                    CFTrackers();
                int entity = GetTrackerFromID(trackerID).Entity;
                if (GetTrackerFromID(trackerID).Ai)
                    SetEntityAsNoLongerNeeded(ref entity);
                trackers.Remove(GetTrackerFromID(trackerID));
            }
            else
                TriggerEvent("chatMessage", "Star Pursuit", new[] { 255, 0, 0 }, "Invalid Tracker ID");
            return;
        }

        private void ETracker(int trackerID)
        {
            if (GetTrackerFromID(trackerID) != null)
            {
                ATracker(GetTrackerFromID(trackerID).Entity, GetTrackerFromID(trackerID).TrackerID);
                TriggerEvent("chatMessage", "Star Pursuit", new[] { 255, 0, 0 }, "Attached to Tracker #" + GetTrackerFromID(trackerID).TrackerID);
            }
            else
                TriggerEvent("chatMessage", "Star Pursuit", new[] { 255, 0, 0 }, "Invalid Tracker ID");
            return;
        }

        private void CTrackers()
        {
            CFTrackers();
            foreach(Tracker tracker in trackers)
            {
                CTracker(tracker.TrackerID);
            }
            TriggerEvent("chatMessage", "Star Pursuit", new[] { 255, 0, 0 }, "Deleted All Trackers");
        }

        private void DTracker()
        {
            if (currentTracker != null)
            {
                TriggerEvent("chatMessage", "Star Pursuit", new[] { 255, 0, 0 }, "Detached from Tracker #" + currentTracker.TrackerID);
                CFTrackers();
            }
            else
            {
                TriggerEvent("chatMessage", "Star Pursuit", new[] { 255, 0, 0 }, "Not attached to any Trackers!");
            }
        }

        private void LTrackers()
        {
            foreach (Tracker tracker in trackers)
            {
                if (tracker.TrackerID != 99999)
                    TriggerEvent("chatMessage", "", new[] { 255, 0, 0 }, "Tracker #" + tracker.TrackerID);
            }
            if (trackers.Count == 0)
                TriggerEvent("chatMessage", "", new[] { 255, 0, 0 }, "No trackers available!");
        }

        private void CFTrackers()
        {
            ClearBlips();
            isTracking = false;
            currentTracker = null;
        }

        private void ATracker(int entity, int id)
        {            
            ClearBlips();
            Vector3 tracker = GetEntityCoords(entity, true);
            Blip blip = new Blip(AddBlipForCoord(tracker.X, tracker.Y, tracker.Z))
            {
                Color = BlipColor.Blue,
                Name = "Tracker #" + id
            };
            SetBlipRoute(blip.Handle, true);
            blipPool.Add(blip);
            isTracking = true;
            currentTracker = GetTrackerFromID(id);
            UpdatePositions(blip, entity, id);
        }

        private async void UpdatePositions(Blip blip, int tracker, int id)
        {
            while (isTracking)
            {
                await Delay(500);
                if (DoesEntityExist(tracker) && isTracking)
                {
                    Vector3 entityPosition = GetEntityCoords(tracker, true);
                    blip.Position = entityPosition;
                    SetBlipRoute(blip.Handle, true);                    
                }                    
            }
        }

        private void ClearBlips()
        {
            foreach (Blip blip in blipPool)
            {
                blip.Delete();
            }
            blipPool.Clear();
        }

        private Tracker GetTrackerFromID(int id)
        {
            Tracker trackerToReturn = null;
            foreach (Tracker tracker in trackers)
            {
                if (tracker.TrackerID == id)
                    trackerToReturn = tracker;
            }
            return trackerToReturn;
        }

        private bool IsTracked(int entity)
        {
            foreach(Tracker tracker in trackers)
            {
                if (tracker.Entity == entity)
                    return true;
            }
            return false;
        }

        private async void DeployTimer()
        {
            timeOut = true;
            await Delay(1000);
            timeOut = false;
        }

        private async Task OnTick()
        {
            await Delay(1);
            if (IsControlPressed(1, 37) && IsPedSittingInAnyVehicle(GetPlayerPed(PlayerId())) && GetVehicleClass(GetVehiclePedIsIn(GetPlayerPed(PlayerId()), false)) == 18 && !timeOut)
            {
                Vector3 ForwardPosition = GetOffsetFromEntityInWorldCoords(GetVehiclePedIsIn(GetPlayerPed(PlayerId()), false), 0, 30, 0);
                int player = GetPlayerPed(PlayerId());
                Vector3 playerCoords = GetEntityCoords(player, true);
                int rayCastPoint = CastRayPointToPoint(playerCoords.X, playerCoords.Y, playerCoords.Z, ForwardPosition.X, ForwardPosition.Y, ForwardPosition.Z, 10, player, 0);
                bool hit = false;
                Vector3 endCoords = new Vector3(0,0,0);
                Vector3 surfaceNormal = new Vector3(0,0,0);
                int entity = 0;
                GetRaycastResult(rayCastPoint, ref hit, ref endCoords, ref surfaceNormal, ref entity);
                GetEntityPlayerIsFreeAimingAt(PlayerId(), ref entity);
                DeployTimer();
                if (entity != 0)
                {
                    if (!IsTracked(entity))
                    {
                        Tracker newTracker = new Tracker
                        {
                            Entity = entity,
                            TrackerID = trackers.Count
                        };
                        if (!IsPedAPlayer(GetPedInVehicleSeat(entity, -1)))
                        {
                            SetEntityAsMissionEntity(entity, true, true);
                            newTracker.Ai = true;
                        }
                        trackers.Add(newTracker);
                        ETracker(newTracker.TrackerID);
                    }
                    else
                    {
                        TriggerEvent("chatMessage", "Star Pursuit", new[] { 255, 0, 0 }, "This vehicle is already being tracked!");
                    }
                }
            }
        }
    }
}
