﻿namespace Microsoft.ApplicationInsights.Extensibility.PerfCounterCollector.Implementation.StandardPerformanceCollector
{
    using System;
    using System.Diagnostics;
    using System.Globalization;

    /// <summary>
    /// Represents normalized value of CPU Utilization by Process counter value (divided by the processors count).
    /// </summary>
    internal class NormalizedProcessCPUPerformanceCounter : ICounterValue, IDisposable
    {
        private readonly int processorsCount;
        private readonly bool isInitialized = false;
        private PerformanceCounter performanceCounter = null;
        
        /// <summary>
        ///  Initializes a new instance of the <see cref="NormalizedProcessCPUPerformanceCounter" /> class.
        /// </summary>
        /// <param name="categoryName">The counter category name.</param>
        /// <param name="counterName">The counter name.</param>
        /// <param name="instanceName">The instance name.</param>
        internal NormalizedProcessCPUPerformanceCounter(string categoryName, string counterName, string instanceName)
        {
            int? count = PerformanceCounterUtility.GetProcessorCount(false);

            if (count.HasValue)
            {
                this.processorsCount = count.Value;

                this.performanceCounter = new PerformanceCounter("Process", "% Processor Time", instanceName, true);

                this.isInitialized = true;
            }
        }

        /// <summary>
        /// Returns the current value of the counter as a <c ref="MetricTelemetry"/>.
        /// </summary>
        /// <returns>Value of the counter.</returns>
        public double Collect()
        {
            if (!this.isInitialized)
            {
                return 0;
            }

            try
            {   
                return this.performanceCounter.NextValue() / this.processorsCount;
            }
            catch (Exception e)
            {
                throw new InvalidOperationException(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        Resources.PerformanceCounterReadFailed,
                        PerformanceCounterUtility.FormatPerformanceCounter(this.performanceCounter)),
                    e);
            }
        }

        /// <summary>
        /// Disposes resources allocated by this type.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose implementation.
        /// </summary>
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.performanceCounter != null)
                {
                    this.performanceCounter.Dispose();
                    this.performanceCounter = null;
                }
            }
        }
    }
}
