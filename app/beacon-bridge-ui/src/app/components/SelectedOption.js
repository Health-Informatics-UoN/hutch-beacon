import Button from "./Button";
import { FcFullTrash } from "react-icons/fc";

export default function SelectedOption({ option, removeFunction }) {
  const deleteIcon = new FcFullTrash();
  return (
    <div className="space-x-2 border-uon-blue-60 border-2 rounded-lg my-2">
      <span className={"flex items-center space-x-1"}>
        <p className="grow">{`${option.id} - ${option.label}`}</p>
        <Button
          icon={deleteIcon}
          text={"Remove"}
          onClick={() => removeFunction(option)}
          className="w-24 bg-uon-red-100 px-2 py-2 rounded-lg text-white"
        />
      </span>
    </div>
  );
}
