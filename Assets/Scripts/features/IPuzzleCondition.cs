using UnityEngine;

namespace JayaJayaJaya.Features
{
    /// <summary>
    /// Interface untuk kondisi puzzle yang harus dipenuhi sebelum route/pintu terbuka
    /// </summary>
    public interface IPuzzleCondition
    {
        /// <summary>
        /// Cek apakah kondisi puzzle sudah terpenuhi
        /// </summary>
        /// <returns>True jika kondisi terpenuhi, false jika belum</returns>
        bool IsConditionMet();
        
        /// <summary>
        /// Nama/deskripsi dari kondisi puzzle
        /// </summary>
        string ConditionName { get; }
    }
}
