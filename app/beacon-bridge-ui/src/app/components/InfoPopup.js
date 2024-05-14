import { FcCheckmark } from "react-icons/fc";
import { FcHighPriority } from "react-icons/fc";

export default function InfoPopup({text, isWarning}) {
  const tick = new FcCheckmark()
  const bang = new FcHighPriority()

  var popupStyle = `border-2 border-white border-solid ${isWarning ? " bg-red-200" : " bg-green-200"}`
  var textColour = isWarning ? "text-red-900" : "text-green-900"
  return (
    <div className={popupStyle}>
      <span className="flex items-center space-x-1">
        {isWarning ? bang : tick}
        <p className={textColour}>{text}</p>
      </span>
    </div>
  )
}
