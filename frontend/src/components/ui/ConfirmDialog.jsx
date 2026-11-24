import BaseButton from "./BaseButton";
import Modal from "./Modal";

export default function ConfirmDialog({
  open, title, children, confirmLabel = "Confirmar", cancelLabel = "Cancelar",
  busy = false, onConfirm, onClose,
}) {
  return (
    <Modal
      open={open}
      title={title}
      onClose={() => !busy && onClose?.()}
      footer={
        <>
          <BaseButton variant="secondary" onClick={onClose} disabled={busy}>{cancelLabel}</BaseButton>
          <BaseButton onClick={onConfirm} loading={busy}>{confirmLabel}</BaseButton>
        </>
      }
    >
      {children}
    </Modal>
  );
}