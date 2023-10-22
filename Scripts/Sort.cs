public class Sort
{
    #region 등급별 분류
    public int ByGrade(CardData cardA, CardData cardB)
    {
        int gradeA = new Convert().GradeToInt(cardA.Grade);
        int gardeB = new Convert().GradeToInt(cardB.Grade);
        int gradeComparison = CompareGrade(gradeA, gardeB);

        if (gradeComparison == 0)
        {
            int typeComparison = CompareType(cardA.Type, cardB.Type);

            if (typeComparison == 0)
            {
                return cardB.Name.CompareTo(cardA.Name);
            }

            return cardA.Type.CompareTo(cardB.Type);
        }
        return gradeA.CompareTo(gardeB);
    }
    int CompareGrade(int gradeA, int gradeB)
    {
        return gradeA.CompareTo(gradeB);
    }
    int CompareType(string typeA, string typeB)
    {
        return typeA.CompareTo(typeB);
    }
    #endregion

    #region 종퓨별 분류
    public int ByType(CardData cardA, CardData cardB)
    {
        return 0;
    }
    #endregion

    #region 레벨별 분류
    public int ByLevel(CardData cardA, CardData cardB)
    {
        return 0;
    }
    #endregion
}
