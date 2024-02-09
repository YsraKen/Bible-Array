using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VersionSelectItem : MonoBehaviour
{
	[SerializeField] TMP_Text _tmp;
	[SerializeField] Toggle _favToggle;
	
	private Version target;
	
	private VersionSelectPanel _panel;
	private VersionSelectPanel panel
	{
		get
		{
			if(!_panel)
				_panel = GetComponentInParent<VersionSelectPanel>();
			
			return _panel;
		}
	}
	
	public void Setup(Version target, bool isFavorite = false)
	{
		this.target = target;		
		_tmp.text = $"{target.Name} ({target.NameCode})";
		
		_favToggle.SetIsOnWithoutNotify(isFavorite);
	}
	
	public void OnSelect() => panel.OnItemSelected(target);
	public void AddToFavorites(bool isAdding) => panel.AddFavoriteVersion(target, isAdding);
}