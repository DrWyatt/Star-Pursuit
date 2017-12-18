using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using starPursuitShared;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;

namespace starPursuitClient
{
    public class StarPursuitClient: BaseScript
    {
        List<Tracker> trackers = new List<Tracker>();
        Tracker currentTracker;
        List<Blip> blipPool = new List<Blip>();
        bool isTracking = false;

        public StarPursuitClient()
        {
            Tick += OnTick;
            EventHandlers.Add("cop:lTrackers", new Action(LTrackers));
            EventHandlers.Add("cop:dTracker", new Action<int>(DTracker));
            EventHandlers.Add("cop:cTrackers", new Action(CTrackers));
            EventHandlers.Add("cop:eTracker", new Action<int>(ETracker));
            EventHandlers.Add("cop:cTracker", new Action<int>(CTracker));
        }

        private void CTracker(int trackerID)
        {
            if (GetTrackerFromID(trackerID) != null)
            {
                TriggerEvent("chatMessage", "Star Pursuit", new[] { 255, 0, 0 }, "Deleted Tracker #" + GetTrackerFromID(trackerID).TrackerID);
                if (isTracking && currentTracker == GetTrackerFromID(trackerID))
                    CFTrackers();
                int entity = GetTrackerFromID(trackerID).entity;
                if (GetTrackerFromID(trackerID).ai)
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
                ATracker(GetTrackerFromID(trackerID).entity, GetTrackerFromID(trackerID).TrackerID);
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

        private void DTracker(int trackerID)
        {
            if (GetTrackerFromID(trackerID) != null)
            {
                CFTrackers();
                TriggerEvent("chatMessage", "Star Pursuit", new[] { 255, 0, 0 }, "Detached from Tracker #" + GetTrackerFromID(trackerID).TrackerID);
            }
            else
                TriggerEvent("chatMessage", "Star Pursuit", new[] { 255, 0, 0 }, "Invalid Tracker ID");
            return;
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

        private async Task OnTick()
        {
            await Delay(500);
            if (IsControlPressed(1, 37) && IsPedSittingInAnyVehicle(GetPlayerPed(PlayerId())) && GetVehicleClass(GetVehiclePedIsIn(GetPlayerPed(PlayerId()), false)) == 18)
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
                if (entity != 0)
                {
                    Tracker newTracker = new Tracker
                    {
                        entity = entity,
                        TrackerID = trackers.Count
                    };
                    if (!IsPedAPlayer(GetPedInVehicleSeat(entity, -1)))
                    {
                        SetEntityAsMissionEntity(entity, true, true);
                        newTracker.ai = true;
                    }
                    trackers.Add(newTracker);
                    ETracker(newTracker.TrackerID);                    
                }
            }
        }
    }
}
