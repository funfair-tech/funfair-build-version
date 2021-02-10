namespace FunFair.BuildVersion.Interfaces
{
    /// <summary>
    ///     Branch Classification
    /// </summary>
    public interface IBranchClassification
    {
        /// <summary>
        ///     Checks to see if the
        /// </summary>
        /// <param name="branchName"></param>
        /// <returns></returns>
        bool IsReleaseBranch(string branchName);
    }
}