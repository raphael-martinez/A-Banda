using Playmove.Core;
using Playmove.Core.API.Models;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Playmove.Avatars.API.Interfaces
{
    public interface IPlayerThumbnail
    {
        long Id { get; }
        string GUID { get; }
        DateTime? LastTimePlayed { get; set; }
        string Name { get; set; }
        List<Classroom> Classrooms { get; set; }
        string ThumbnailPath { get; set; }

        void GetThumbnailSprite(AsyncCallback<Sprite> completed);
        void GetAvatarSprite(AsyncCallback<Sprite> completed);
    }
}
