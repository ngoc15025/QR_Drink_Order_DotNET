using System;
using WebPush;

var vapidDetails = VapidHelper.GenerateVapidKeys();
Console.WriteLine($"PublicKey: {vapidDetails.PublicKey}");
Console.WriteLine($"PrivateKey: {vapidDetails.PrivateKey}");
