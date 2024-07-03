namespace CargoSim.Application.Extensions;

public static class ListExtensions
{
    public static bool ContainsSublist<T>(this List<T> currentList, List<T> subList)
    {
        if (subList.Count == 0)
            return true; // TODO : throw

        if (currentList.Count < subList.Count)
            return false;

        for (int i = 0; i <= currentList.Count - subList.Count; i++) //0,1,2
        {
            bool isSublist = true;

            for (int j = 0; j < subList.Count; j++) // 0,1,2
            {
                if (!currentList[i + j].Equals(subList[j]))
                {
                    isSublist = false;

                    break;
                }
            }

            if (isSublist)
            {
                return true;
            }
        }

        return false;
    }
}
