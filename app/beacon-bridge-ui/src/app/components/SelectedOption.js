import Button from "./Button"
import { FcFullTrash } from "react-icons/fc"

export default function SelectedOption({option, removeFunction}) {
  const deleteIcon = new FcFullTrash()
  return (
    <div className="space-x-2 border-uon-primary-80 border-2">
      <span className={"flex items-center space-x-1"}>
        <p className="grow">{`${option.id} - ${option.label}`}</p>
        <Button icon={deleteIcon} text={"Remove"} onClick={() => removeFunction(option)}/>
      </span>
    </div>
  )
}
