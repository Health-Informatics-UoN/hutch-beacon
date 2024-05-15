import { FcCheckmark } from "react-icons/fc";
import { FcHighPriority } from "react-icons/fc";

export default function InfoPopup({ text, isWarning }) {
  const tick = new FcCheckmark();
  const bang = new FcHighPriority();

  var popupStyle = `rounded-lg border-2 border-white border-solid ${isWarning ? " bg-uon-red-20" : " bg-uon-bramley-20"}`;
  var textColour = isWarning ? "text-uon-red-100" : "text-uon-bramley-100";
  return (
    <div className={popupStyle}>
      <span className="flex items-center space-x-1">
        {isWarning ? bang : tick}
        <p className={textColour}>{text}</p>
      </span>
    </div>
  );
}
