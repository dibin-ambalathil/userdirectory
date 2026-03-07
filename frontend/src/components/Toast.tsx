interface ToastProps {
  message: string;
  onClose: () => void;
}

export function Toast({ message, onClose }: ToastProps): JSX.Element {
  return (
    <div className="toast" role="alert">
      <span>{message}</span>
      <button type="button" className="toast-close" onClick={onClose} aria-label="Close toast">
        x
      </button>
    </div>
  );
}
