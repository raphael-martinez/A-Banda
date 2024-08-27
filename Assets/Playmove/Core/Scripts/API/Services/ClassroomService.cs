using System.Collections.Generic;
using System.Linq;
using Playmove.Core.API.Models;
using Playmove.Core.API.Vms;

namespace Playmove.Core.API.Services
{
    /// <summary>
    /// Responsible to access APIs for Classroom
    /// </summary>
    public class ClassroomService : Service<Classroom, TurmaVm>
    {
        public List<Classroom> Classrooms { get; private set; } = new List<Classroom>();

        /// <summary>
        /// Get specific classroom based on it's id
        /// </summary>
        /// <param name="id">Id of the classroom</param>
        /// <param name="completed">Callback containing the Classroom or error</param>
        public void Get(long id, AsyncCallback<Classroom> completed)
        {
            WebRequestWrapper.Instance.Get("/Turmas/Get", new Dictionary<string, string> { { "id", id.ToString() } },
                result => completed?.Invoke(ParseVmJson(result)));
        }
        /// <summary>
        /// Get specific classroom based on it's GUID
        /// </summary>
        /// <param name="GUID">GUID of the classroom</param>
        /// <param name="completed">Callback containing the Classroom or error</param>
        public void Get(string GUID, AsyncCallback<Classroom> completed)
        {
            completed?.Invoke(new AsyncResult<Classroom>(null, "Not implemented yet!"));
        }
        /// <summary>
        /// Get all classrooms available
        /// </summary>
        /// <param name="completed">Callback containing a list with all Classrooms or error</param>
        public void GetAll(AsyncCallback<List<Classroom>> completed)
        {
            if (Classrooms.Count > 0)
            {
                completed?.Invoke(new AsyncResult<List<Classroom>>(Classrooms, string.Empty));
                return;
            }

            WebRequestWrapper.Instance.Get("/Turmas/GetAll", result =>
            {
                AsyncResult<List<Classroom>> parsedResult = ParseVmsJson(result);
                if (parsedResult.Success)
                {
                    parsedResult.Data = parsedResult.Data.Where(classroom => !classroom.Deleted).ToList();
                    Classrooms = parsedResult.Data;
                }
                completed?.Invoke(parsedResult);
            });
        }
    }
}
