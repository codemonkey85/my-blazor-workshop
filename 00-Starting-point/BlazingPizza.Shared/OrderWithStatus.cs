﻿using System;
using System.Collections.Generic;
using BlazingPizza.ComponentsLibrary.Map;

namespace BlazingPizza
{
    public class OrderWithStatus
    {
        public Order Order
        {
            get; set;
        }

        public string StatusText
        {
            get; set;
        }

        public List<Marker> MapMarkers
        {
            get; set;
        }

        public static OrderWithStatus FromOrder(Order order)
        {
            // To simulate a real backend process, we fake status updates based on the amount
            // of time since the order was placed
            string statusText;
            List<Marker> mapMarkers;
            DateTime dispatchTime = order.CreatedTime.AddSeconds(10);
            TimeSpan deliveryDuration = TimeSpan.FromMinutes(1); // Unrealistic, but more interesting to watch

            if (DateTime.Now < dispatchTime)
            {
                statusText = "Preparing";
                mapMarkers = new List<Marker>
                {
                    ToMapMarker("You", order.DeliveryLocation, showPopup: true)
                };
            }
            else if (DateTime.Now < dispatchTime + deliveryDuration)
            {
                statusText = "Out for delivery";

                LatLong startPosition = ComputeStartPosition(order);
                double proportionOfDeliveryCompleted = Math.Min(1, (DateTime.Now - dispatchTime).TotalMilliseconds / deliveryDuration.TotalMilliseconds);
                LatLong driverPosition = LatLong.Interpolate(startPosition, order.DeliveryLocation, proportionOfDeliveryCompleted);
                mapMarkers = new List<Marker>
                {
                    ToMapMarker("You", order.DeliveryLocation),
                    ToMapMarker("Driver", driverPosition, showPopup: true),
                };
            }
            else
            {
                statusText = "Delivered";
                mapMarkers = new List<Marker>
                {
                    ToMapMarker("Delivery location", order.DeliveryLocation, showPopup: true),
                };
            }

            return new OrderWithStatus
            {
                Order = order,
                StatusText = statusText,
                MapMarkers = mapMarkers,
            };
        }

        private static LatLong ComputeStartPosition(Order order)
        {
            // Random but deterministic based on order ID
            Random rng = new Random(order.OrderId);
            double distance = 0.01 + rng.NextDouble() * 0.02;
            double angle = rng.NextDouble() * Math.PI * 2;
            (double, double) offset = (distance * Math.Cos(angle), distance * Math.Sin(angle));
            return new LatLong(order.DeliveryLocation.Latitude + offset.Item1, order.DeliveryLocation.Longitude + offset.Item2);
        }

        private static Marker ToMapMarker(string description, LatLong coords, bool showPopup = false)
            => new Marker { Description = description, X = coords.Longitude, Y = coords.Latitude, ShowPopup = showPopup };
    }
}