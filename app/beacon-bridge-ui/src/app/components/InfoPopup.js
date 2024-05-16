import { FcCheckmark } from "react-icons/fc";
import { FcHighPriority } from "react-icons/fc";

export default function InfoPopup({ text, isWarning, ...props }) {
  const tick = new FcCheckmark();
  const bang = new FcHighPriority();

  var buttonColour = isWarning ? "bg-uon-red-20" : "bg-uon-bramley-20";
  var textColour = isWarning ? "text-uon-red-100" : "text-uon-forest-80";
  return (
    <div className={`${props.className} ${buttonColour} ${textColour}`}>
      <span className="flex items-center space-x-1">
        {isWarning ? bang : tick}
        <p className={textColour}>{text}</p>
      </span>
    </div>
  );
}
