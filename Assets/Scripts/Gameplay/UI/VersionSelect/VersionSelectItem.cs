using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VersionSelectItem : MonoBehaviour
{
	[SerializeField] TMP_Text _tmp;
	[SerializeField] Toggle _favToggle;
	
	public Version Target { get; private set; }
	
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
	
	public void Setup(Version Target, bool isFavorite = false)
	{
		this.Target = Target;
		_tmp.text = $"{Target.Name} ({Target.NameCode})";
		
		_favToggle.SetIsOnWithoutNotify(isFavorite);
	}
	
	public void OnSelect() => panel.OnItemSelected(Target);
	public void AddToFavorites(bool isAdding) => panel.AddFavoriteVersion(Target, isAdding);
}