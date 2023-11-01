﻿using System;

namespace SWUploaderNet50
{
    internal class SubmissionProgress : IProgress<float>
    {
        private float lastValue = 0;

        public void Report(float value)
        {
            if (value > lastValue)
            {
                lastValue = value;
                // TODO: Update GUI
            }
        }
    }
}
