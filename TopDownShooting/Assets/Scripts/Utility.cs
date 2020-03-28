using System.Collections;

public static class Utility
{
    // 제네릭 타입 이용해서 배열 받아서 셔플 해주기
    // seed : 랜덤 값을 만드는데 기준이 되는 초기값
    public static T[] ShuffleArray<T>(T[] array, int seed)
    {
        System.Random prng = new System.Random(seed);

        // 마지막 루프 생략 가능 >> -1
        for(int i=0; i<array.Length - 1; i++)
        {
            int randomIndex = prng.Next(i, array.Length);
            T tempItem = array[randomIndex];
            array[randomIndex] = array[i];
            array[i] = tempItem;
        }

        return array;
    }
}
