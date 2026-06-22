# Spawn Channel And Pose Resolver Guide

Placement is abstract:

`WorldSpawnChannelId -> ISpawnPoseResolver -> SpawnPose`

The default helper `ChannelPoseResolver` maps channels to fixed poses. Products should provide their own resolver for:

- perimeter or radial placement
- lane entries
- portals
- grid cells
- authored scene markers
- procedural regions

The package core does not contain radial math, lane pathing, tower placement, center-core assumptions, or path nodes.
