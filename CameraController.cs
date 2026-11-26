using Godot;

public partial class CameraController : Camera3D
{
	// How fast the camera moves with WASD (units per second).
	[Export] public float MoveSpeed { get; set; } = 5.0f;

	// Mouse sensitivity for right-drag looking.
	[Export] public float MouseSensitivity { get; set; } = 0.003f;

	// Zoom limits (distance along the camera's local -Z when scrolling).
	[Export] public float MinZoom { get; set; } = 1.0f;
	[Export] public float MaxZoom { get; set; } = 20.0f;

	// How much one scroll notch changes the zoom (in world units).
	[Export] public float ZoomStep { get; set; } = 1.0f;

	// Current zoom distance. The camera sits this far back along its local -Z.
	private float _zoom = 5.0f;

	// Accumulated look rotation (radians) applied while right-dragging.
	private float _yaw;
	private float _pitch;

	public override void _Ready()
	{
		// Start from the camera's existing orientation so the scene transform still matters.
		_yaw = Rotation.Y;
		_pitch = Rotation.X;
		ApplyZoom();
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		// Scroll wheel zooms in/out.
		if (@event is InputEventMouseButton mouseButton)
		{
			if (mouseButton.Pressed)
			{
				if (mouseButton.ButtonIndex == MouseButton.WheelUp)
				{
					_zoom = Mathf.Clamp(_zoom - ZoomStep, MinZoom, MaxZoom);
					ApplyZoom();
				}
				else if (mouseButton.ButtonIndex == MouseButton.WheelDown)
				{
					_zoom = Mathf.Clamp(_zoom + ZoomStep, MinZoom, MaxZoom);
					ApplyZoom();
				}
			}
		}
		else if (@event is InputEventMouseMotion mouseMotion && Input.IsMouseButtonPressed(MouseButton.Right))
		{
			// Right-drag to look around.
			_yaw -= mouseMotion.Relative.X * MouseSensitivity;
			_pitch -= mouseMotion.Relative.Y * MouseSensitivity;
			// Keep the camera from flipping over.
			_pitch = Mathf.Clamp(_pitch, -1.5f, 1.5f);
			Rotation = new Vector3(_pitch, _yaw, 0);
		}
	}

	public override void _Process(double delta)
	{
		// WASD moves the camera in its own local space.
		var dir = Vector3.Zero;
		if (Input.IsActionPressed("ui_up") || Input.IsPhysicalKeyPressed(Key.W))
			dir += -Transform.Basis.Z;
		if (Input.IsActionPressed("ui_down") || Input.IsPhysicalKeyPressed(Key.S))
			dir += Transform.Basis.Z;
		if (Input.IsActionPressed("ui_left") || Input.IsPhysicalKeyPressed(Key.A))
			dir += -Transform.Basis.X;
		if (Input.IsActionPressed("ui_right") || Input.IsPhysicalKeyPressed(Key.D))
			dir += Transform.Basis.X;

		if (dir != Vector3.Zero)
		{
			// Keep movement horizontal (ignore pitch when flying forward).
			dir.Y = 0;
			Position += dir.Normalized() * MoveSpeed * (float)delta;
		}
	}

	// Positions the camera along its local -Z at the current zoom distance.
	private void ApplyZoom()
	{
		Position = new Vector3(Position.X, Position.Y, -_zoom);
	}
}