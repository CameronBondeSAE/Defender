using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

public class AuthenticationManager : MonoBehaviour
{
	public event Action OnSignedIn;

	public async Task SignInAsync()
	{
		try
		{
			await UnityServices.InitializeAsync();
			SetupAuthenticationEvents();
			if (!AuthenticationService.Instance.IsSignedIn)
			{
				await AuthenticationService.Instance.SignInAnonymouslyAsync();
				OnSignedIn?.Invoke();
			}
		}
		catch (Exception e)
		{
			Debug.LogException(e);
		}
		
	}

	void SetupAuthenticationEvents()
	{
		// Setup authentication event handlers if desired
		AuthenticationService.Instance.SignedIn += () =>
		                                           {
			                                           // Shows how to get a playerID
			                                           Debug
				                                           .Log($"PlayerID: {AuthenticationService.Instance.PlayerId}");

			                                           // Shows how to get an access token
			                                           Debug
				                                           .Log($"Access Token: {AuthenticationService.Instance.AccessToken}");
		                                           };

		AuthenticationService.Instance.SignInFailed += (err) => { Debug.LogError(err); };

		AuthenticationService.Instance.SignedOut += () => { Debug.Log("Player signed out."); };

		AuthenticationService.Instance.Expired += () =>
		                                          {
			                                          Debug
				                                          .Log("Player session could not be refreshed and expired.");
		                                          };
	}
}
