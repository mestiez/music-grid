using Shared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MusicGrid
{
    public class TrackQueue
    {
        private int currentIndex;
        private int[] shuffleIndices = { };
        private bool shuffle = false;
        private readonly Random random = new Random();

        public List<DistrictEntry> Tracks { get; private set; } = new List<DistrictEntry>();
        public DistrictEntry CurrentTrack => Shuffle ? Tracks[shuffleIndices[CurrentIndex]] : CurrentTrackRaw;
        public DistrictEntry CurrentTrackRaw => Tracks.Count == 0 ? null : Tracks[CurrentIndex];

        public bool Shuffle
        {
            get => shuffle;

            set
            {
                OnShuffleChange?.Invoke(this, value);
                shuffle = value;
            }
        }

        public event EventHandler<DistrictEntry> OnTrackChange;
        public event EventHandler OnModification;
        public event EventHandler<bool> OnShuffleChange;

        public int CurrentIndex
        {
            get => currentIndex;
            set
            {
                currentIndex = value;
                if (currentIndex < 0) currentIndex = Tracks.Count - 1;
                if (currentIndex >= Tracks.Count) currentIndex = 0;

                OnTrackChange?.Invoke(this, CurrentTrack);
            }
        }

        public void SkipToOrEnqueue(DistrictEntry entry)
        {
            int index = Tracks.IndexOf(entry);
            if (index == -1)
                Enqueue(entry);

            CurrentIndex = Shuffle ? GetShuffledIndexOf(entry) : Tracks.IndexOf(entry);
        }

        public void Enqueue(DistrictEntry entry)
        {
            Tracks.Add(entry);
            RepopulateShuffleIndices();
        }

        public void Dequeue(DistrictEntry entry)
        {
            Tracks.Remove(entry);
            RepopulateShuffleIndices();
        }

        public void Enqueue(District district)
        {
            foreach (var entry in district.Entries)
                Tracks.Add(entry);
            RepopulateShuffleIndices();
        }

        public void ClearQueue()
        {
            Tracks.Clear();
            RepopulateShuffleIndices();
        }

        public void Next()
        {
            for (int i = 1; i < Tracks.Count - 1; i++)
            {
                int index = LoopBackIndex(CurrentIndex + i);
                if (!GetTrackAt(index)?.District?.Muted ?? false)
                {
                    CurrentIndex = index;
                    return;
                }
            }
        }

        public void Previous()
        {
            for (int i = 1; i < Tracks.Count - 1; i++)
            {
                int index = LoopBackIndex(CurrentIndex - i);
                if (!GetTrackAt(index)?.District?.Muted ?? false)
                {
                    CurrentIndex = index;
                    return;
                }
            }
        }

        private DistrictEntry GetTrackAt(int index)
        {
            index = LoopBackIndex(index);
            if (Shuffle) return Tracks[shuffleIndices[index]];
            return Tracks[index];
        }

        private int LoopBackIndex(int index)
        {
            if (index < 0) index = Tracks.Count - (Math.Abs(index) % Tracks.Count);
            if (index >= Tracks.Count) index %= Tracks.Count;
            return index;
        }

        private int GetShuffledIndexOf(DistrictEntry entry)
        {
            int index = Tracks.IndexOf(entry);
            if (index == -1)
                return -1;

            return Array.IndexOf(shuffleIndices, index);
        }

        public void RepopulateShuffleIndices()
        {
            shuffleIndices = new int[Tracks.Count];
            for (int i = 0; i < shuffleIndices.Length; i++)
                shuffleIndices[i] = i;
            shuffleIndices = shuffleIndices.OrderBy(i => random.Next(0, Tracks.Count)).ToArray();
            OnModification?.Invoke(this, EventArgs.Empty);
        }
    }
}
