namespace MiniBank.Utils;

public static class ConsultaUtils
{
    public static T? MaiorPor<T, TKey>(IEnumerable<T> colecao, Func<T, TKey> seletor)
        where TKey : IComparable<TKey>
    {
        return colecao.OrderByDescending(seletor).FirstOrDefault();
    }

    public static Dictionary<TKey, List<T>> AgruparPor<T, TKey>(IEnumerable<T> colecao, Func<T, TKey> seletor)
        where TKey : notnull
    {
        var resultado = new Dictionary<TKey, List<T>>();

        foreach (var item in colecao)
        {
            var chave = seletor(item);
            if (!resultado.ContainsKey(chave))
            {
                resultado[chave] = [];
            }

            resultado[chave].Add(item);
        }

        return resultado;
    }
}
