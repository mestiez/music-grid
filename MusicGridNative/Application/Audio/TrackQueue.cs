using Shared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MusicGrid
{
    public class TrackQueue
    {
        private int currentIndex;
        private int[] shuffleIndices;
        private readonly Random random = new Random();

        public List<DistrictEntry> Tracks { get; private set; } = new List<DistrictEntry>();
        public DistrictEntry CurrentTrack => Tracks[shuffleIndices[CurrentIndex]];
        public DistrictEntry CurrentTrackRaw => Tracks[CurrentIndex];
        public bool Shuffle { get; set; } = true;

        public event EventHandler<DistrictEntry> OnTrackChange;
        public event EventHandler OnModification;

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
            CurrentIndex++;
        }

        public void Previous()
        {
            CurrentIndex--;
        }

        private int GetShuffledIndexOf(DistrictEntry entry)
        {
            int index = Tracks.IndexOf(entry);
            if (index == -1)
                return -1;

            return Array.IndexOf(shuffleIndices, index);
        }

        private void RepopulateShuffleIndices()
        {
            shuffleIndices = new int[Tracks.Count];
            for (int i = 0; i < shuffleIndices.Length; i++)
                shuffleIndices[i] = i;
            shuffleIndices = shuffleIndices.OrderBy(i => random.Next(0, 100)).ToArray();
            OnModification?.Invoke(this, EventArgs.Empty);
        }
    }
}
