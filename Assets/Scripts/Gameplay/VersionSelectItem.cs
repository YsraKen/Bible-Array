using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VersionSelectItem : MonoBehaviour
{
	[SerializeField] TMP_Text _tmp;
	[SerializeField] Toggle _favToggle;
	
	private Version target;
	
	public void Setup(Version target, bool isFavorite = false)
	{
		this.target = target;		
		_tmp.text = $"{target.Name} ({target.NameCode})";
		
		_favToggle.SetIsOnWithoutNotify(isFavorite);
	}
	
	public void OnSelect() => GameManager.Instance.OnVersionSelect(target);
	
	public void AddToFavorites(bool isAdding) => GameManager.Instance.AddFavoriteVersion(target, isAdding);
}