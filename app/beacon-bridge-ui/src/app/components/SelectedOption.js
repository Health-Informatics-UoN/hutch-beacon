import Button from "./Button";
import { FaRegTrashAlt } from "react-icons/fa";

export default function SelectedOption({ option, removeFunction }) {
  const deleteIcon = new FaRegTrashAlt();
  return (
    <div className="space-x-2 border-uon-blue-60 border-2 rounded-lg my-2">
      <span className={"flex items-center space-x-1"}>
        <p className="grow">{`${option.id} - ${option.label}`}</p>
        <Button
          icon={deleteIcon}
          text={"Remove"}
          onClick={() => removeFunction(option)}
          className="w-24 px-2 py-2 rounded-lg text-uon-red-80"
        />
      </span>
    </div>
  );
}
