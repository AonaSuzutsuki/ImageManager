namespace FileManagerLib.SQLite
{
    public interface IVacuumable
    {
        /// <summary>
        /// Collect the space used by unnecessary tuples.
        /// </summary>
        void Vacuum();
    }
}