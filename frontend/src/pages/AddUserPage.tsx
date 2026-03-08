import { FormEvent, useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { createUser, getApiErrorMessage } from '../api/usersApi';
import { useAuthState } from '../auth/useAuthState';

interface FormValues {
  name: string;
  age: string;
  city: string;
  state: string;
  pincode: string;
}

type ValidationErrors = Partial<Record<keyof FormValues, string>>;

const initialValues: FormValues = {
  name: '',
  age: '',
  city: '',
  state: '',
  pincode: ''
};

function validate(values: FormValues): ValidationErrors {
  const errors: ValidationErrors = {};

  if (!values.name.trim()) {
    errors.name = 'Name is required.';
  } else if (values.name.trim().length < 2 || values.name.trim().length > 100) {
    errors.name = 'Name must be between 2 and 100 characters.';
  }

  const parsedAge = Number(values.age);
  if (values.age.trim().length === 0 || Number.isNaN(parsedAge)) {
    errors.age = 'Age is required.';
  } else if (parsedAge < 0 || parsedAge > 120) {
    errors.age = 'Age must be between 0 and 120.';
  }

  if (!values.city.trim()) {
    errors.city = 'City is required.';
  }

  if (!values.state.trim()) {
    errors.state = 'State is required.';
  }

  if (!values.pincode.trim()) {
    errors.pincode = 'Pincode is required.';
  } else if (values.pincode.trim().length < 4 || values.pincode.trim().length > 10) {
    errors.pincode = 'Pincode must be between 4 and 10 characters.';
  }

  return errors;
}

export function AddUserPage(): JSX.Element {
  const [values, setValues] = useState<FormValues>(initialValues);
  const [errors, setErrors] = useState<ValidationErrors>({});
  const [formError, setFormError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const auth = useAuthState();
  const navigate = useNavigate();

  const hasErrors = useMemo(() => Object.keys(errors).length > 0, [errors]);

  async function handleSubmit(event: FormEvent<HTMLFormElement>): Promise<void> {
    event.preventDefault();

    const validationErrors = validate(values);
    setErrors(validationErrors);
    setFormError(null);

    if (Object.keys(validationErrors).length > 0) {
      return;
    }

    try {
      setIsSubmitting(true);

      await createUser({
        name: values.name.trim(),
        age: Number(values.age),
        city: values.city.trim(),
        state: values.state.trim(),
        pincode: values.pincode.trim()
      });

      navigate('/users', {
        state: {
          toastMessage: 'User added successfully.'
        }
      });
    } catch (requestError) {
      setFormError(getApiErrorMessage(requestError));
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <section className="page-card">
      <header className="page-header">
        <h1>Add User</h1>
        <p>Create a new directory entry.</p>
      </header>

      <form className="form-grid" onSubmit={(event) => void handleSubmit(event)} noValidate>
        <label>
          Name
          <input
            type="text"
            value={values.name}
            onChange={(event) => setValues((current) => ({ ...current, name: event.target.value }))}
            placeholder="Enter name"
          />
          {errors.name ? <span className="input-error">{errors.name}</span> : null}
        </label>

        <label>
          Age
          <input
            type="number"
            value={values.age}
            onChange={(event) => setValues((current) => ({ ...current, age: event.target.value }))}
            placeholder="Enter age"
            min={0}
            max={120}
          />
          {errors.age ? <span className="input-error">{errors.age}</span> : null}
        </label>

        <label>
          City
          <input
            type="text"
            value={values.city}
            onChange={(event) => setValues((current) => ({ ...current, city: event.target.value }))}
            placeholder="Enter city"
          />
          {errors.city ? <span className="input-error">{errors.city}</span> : null}
        </label>

        <label>
          State
          <input
            type="text"
            value={values.state}
            onChange={(event) => setValues((current) => ({ ...current, state: event.target.value }))}
            placeholder="Enter state"
          />
          {errors.state ? <span className="input-error">{errors.state}</span> : null}
        </label>

        <label>
          Pincode
          <input
            type="text"
            value={values.pincode}
            onChange={(event) => setValues((current) => ({ ...current, pincode: event.target.value }))}
            placeholder="Enter pincode"
          />
          {errors.pincode ? <span className="input-error">{errors.pincode}</span> : null}
        </label>

        {formError ? <p className="status-message error">Failed to create user: {formError}</p> : null}

        <div className="button-row">
          <button className="primary-button" type="submit" disabled={isSubmitting}>
            {isSubmitting ? 'Saving...' : 'Save User'}
          </button>
          {hasErrors ? <span className="hint-text">Please all the details.</span> : null}
        </div>
      </form>
    </section>
  );
}
