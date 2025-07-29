using System;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

namespace CameronBonde
{
	public class LobbyManager : MonoBehaviour
	{
		async void Awake()
		{
			try
			{
				await UnityServices.InitializeAsync();
				SetupEvents();
			}
			catch (Exception e)
			{
				Debug.LogException(e);
			}
		}

		// Setup authentication event handlers if desired
		void SetupEvents()
		{
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
}