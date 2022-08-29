# ChilloutVR Autoconnect mod

This mod is designed to give you the abilities of the vrchat:// url in CVR with added functionality


### cvr:// Uri
Here's an example of a [CVR uri](https://github.com/bluscream-cvrmods/AutoConnect/blob/main/Classes/CVRUrl.cs):
```
cvr://launch?id=4b60d4ec-7043-4453-82fe-b976a8500a3c:i+7d523258924e4251-559101-94d7bb-16ca42ed&pos=14.0,2.6,75.6&rot=0.0,0.1,0.0,1.0
```
| Parameter 	| Type                                	| Required 	| Description                                                                                                                                                                                                                 	| Example                                                                               	|
|-----------	|-------------------------------------	|----------	|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------	|---------------------------------------------------------------------------------------	|
| id        	| WorldID<:InstanceID>                	| No       	| This should contain a world id and optionally a instance id to give the mod a active instance to join to after startup.<br>If only a world id is present it will only join the world locally (as if it was your home world) 	| `?rot=4b60d4ec-7043-4453-82fe-b976a8500a3c:i+7d523258924e4251-559101-94d7bb-16ca42ed` 	|
| pos       	| Vector3(float,float,float)          	| No       	| This field contains a comma seperated list of x,y and z coordinates in decimal format that will be applied once you spawn on the world specified above                                                                      	| `?pos=14.0,2.6,75.6`                                                                  	|
| rot       	| Quaternion(float,float,float,float) 	| No       	| This field contains a comma seperated list of x,y,z and w rotation points in decimal format that will be applied once you spawn on the world specified above                                                                	| `?rot=0.0,0.1,0.0,1.0`                                                                	|
