using System;

namespace Playmove.Core.API.Models
{
    public interface ITrash
    {
        long Id { get; }
        string Name { get; }
        bool Deleted { get; set; }
        DateTime? DeletedAt { get; set; }
    }
}
